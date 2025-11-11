using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AccountAPI.DTOs;
using AccountAPI.Models;
using AccountAPI.Repositories.Interfaces;
using AccountAPI.Services.Email;
using AccountAPI.Services.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;

namespace AccountAPI.Services
{
    public class AccountService : IAccountService
    {
        private readonly IUserRepository _repo;
        private readonly IMemoryCache _cache;
        private readonly IMapper _mapper;
        private readonly IEmailSender _email;
        private readonly IConfiguration _cfg;

        public AccountService(
            IUserRepository repo,
            IMemoryCache cache,
            IMapper mapper,
            IEmailSender email,
            IConfiguration cfg)
        {
            _repo = repo;
            _cache = cache;
            _mapper = mapper;
            _email = email;
            _cfg = cfg;
        }

        public async Task<AuthResponseDTO?> LoginAsync(LoginDTO dto)
        {
            var user = await _repo.GetByUsernameAsync(dto.Username);
            if (user == null) return null;
            if (user.IsBanned ?? false) return null;

            if (!IsBcryptHash(user.PasswordHash)) return null;
            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash)) return null;

            var mapped = _mapper.Map<UserDTO>(user);
            var (token, expiresAtUtc) = GenerateJwtToken(user);

            return new AuthResponseDTO
            {
                AccessToken = token,
                ExpiresAtUtc = expiresAtUtc,
                User = mapped
            };
        }

        private static bool IsBcryptHash(string? hash) =>
            !string.IsNullOrWhiteSpace(hash) &&
            (hash!.StartsWith("$2a$") || hash.StartsWith("$2b$") || hash.StartsWith("$2y$"));

        private (string Token, DateTime ExpiresAtUtc) GenerateJwtToken(User user)
        {
            var key = _cfg["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(key))
                throw new InvalidOperationException("JWT signing key is not configured.");

            var issuer = _cfg["Jwt:Issuer"];
            var audience = _cfg["Jwt:Audience"];
            var expiryMinutes = _cfg.GetValue("Jwt:ExpiryMinutes", 120);
            var expires = DateTime.UtcNow.AddMinutes(expiryMinutes);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            };

            var normalizedRole = string.IsNullOrWhiteSpace(user.Role)
                ? "User"
                : CultureInfo.InvariantCulture.TextInfo.ToTitleCase(user.Role.Trim().ToLowerInvariant());
            claims.Add(new Claim(ClaimTypes.Role, normalizedRole));
            claims.Add(new Claim("role", normalizedRole));

            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
            }

            if (!string.IsNullOrWhiteSpace(user.FullName))
            {
                claims.Add(new Claim("full_name", user.FullName));
            }

            if (!string.IsNullOrWhiteSpace(user.AvatarUrl))
            {
                claims.Add(new Claim("avatar_url", user.AvatarUrl));
            }

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expires,
                signingCredentials: credentials);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return (tokenString, expires);
        }

        public async Task<UserDTO> RegisterAsync(RegisterDTO dto)
        {
            dto.Username = dto.Username?.Trim() ?? string.Empty;
            dto.FullName = dto.FullName?.Trim();
            dto.Email = dto.Email?.Trim();
            dto.Phone = dto.Phone?.Trim();

            if (string.IsNullOrWhiteSpace(dto.Username))
                throw new InvalidOperationException("Tên đăng nhập là bắt buộc.");

            if (await _repo.GetByUsernameAsync(dto.Username) != null)
                throw new InvalidOperationException("Tên đăng nhập đã được sử dụng.");

            if (!string.IsNullOrWhiteSpace(dto.Email) &&
                await _repo.GetByEmailAsync(dto.Email) != null)
                throw new InvalidOperationException("Email đã được sử dụng.");

            var user = _mapper.Map<User>(dto);
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            await _repo.AddAsync(user);
            return _mapper.Map<UserDTO>(user);
        }

        public async Task<IEnumerable<UserDTO>> GetAllAsync() =>
            (await _repo.GetAllAsync()).Select(u => _mapper.Map<UserDTO>(u));

        public async Task<UserDTO?> GetByIdAsync(int id)
        {
            var user = await _repo.GetByIdAsync(id);
            return user == null ? null : _mapper.Map<UserDTO>(user);
        }

        public async Task<UserDTO?> UpdateProfileAsync(int userId, UpdateProfileDTO dto, IFormFile? avatarFile)
        {
            var user = await _repo.GetByIdAsync(userId);
            if (user == null) return null;

            _mapper.Map(dto, user);

            if (avatarFile != null && avatarFile.Length > 0)
            {
                var cloudinary = new CloudinaryService(_cfg); // Nếu bạn đang DI, inject vào constructor cho chuẩn hơn
                var url = await cloudinary.UploadImageAsync(avatarFile);
                user.AvatarUrl = url;
            }

            user.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(user);

            return _mapper.Map<UserDTO>(user);
        }

        public async Task<bool> ChangePasswordAsync(ChangePasswordDTO dto)
        {
            var user = await _repo.GetByIdAsync(dto.UserId);
            if (user == null) return false;

            if (!IsBcryptHash(user.PasswordHash)) return false;
            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash)) return false;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(user);
            return true;
        }

        public async Task<bool> SetBanAsync(int userId, bool isBanned)
        {
            var user = await _repo.GetByIdAsync(userId);
            if (user == null) return false;

            user.IsBanned = isBanned;
            user.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(user);
            return true;
        }

        private static string ToBase64Url(byte[] bytes) =>
            Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');

        public async Task<string?> ForgotPasswordAsync(ForgotPasswordDTO dto)
        {
            var user = await _repo.GetByEmailAsync(dto.Email);
            if (user == null) return null;

            var token = ToBase64Url(RandomNumberGenerator.GetBytes(24));
            var cacheKey = $"pwdreset:{token}";

            _cache.Set(cacheKey, user.UserId, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
            });

            var feBase = _cfg["Frontend:BaseUrl"]?.TrimEnd('/') ?? "https://localhost:7223";
            var resetUrl = $"{feBase}/Accounts/ResetPassword?token={Uri.EscapeDataString(token)}";

            var displayName = string.IsNullOrWhiteSpace(user.FullName) ? user.Username : user.FullName;
            var supportName = _cfg["Email:FromName"] ?? "DOS Support Team";
            const int expirationMinutes = 15;
            var currentYear = DateTime.UtcNow.Year;

            var html = $$"""
<div style="margin:0;padding:0;background-color:#f5f5f5;font-family:'Segoe UI',Roboto,Helvetica,Arial,sans-serif;color:#2d2d2d;">
  <table role="presentation" cellpadding="0" cellspacing="0" width="100%">
    <tr>
      <td align="center" style="padding:32px 16px;">
        <table role="presentation" cellpadding="0" cellspacing="0" width="100%" style="max-width:600px;background-color:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 10px 30px rgba(20,20,20,0.08);">
          <tr>
            <td align="center" style="background:linear-gradient(135deg,#1a6cff,#00264d);padding:28px 16px;">
              <a href="{{feBase}}" target="_blank" style="display:inline-flex;align-items:center;text-decoration:none;">
                <span style="font-size:20px;font-weight:700;color:#ffffff;letter-spacing:0.4px;">DrinkOrder System</span>
              </a>
            </td>
          </tr>
          <tr>
            <td style="padding:32px 28px 16px;">
              <h1 style="margin:0 0 18px;font-size:24px;color:#0f1d2d;">Password reset requested</h1>
              <p style="margin:0 0 16px;line-height:1.6;">Hi {{displayName}},</p>
              <p style="margin:0 0 16px;line-height:1.6;">
                We received a request to reset the password for your DrinkOrder System account.
                Click the button below to set a new password. This link is valid for {{expirationMinutes}} minutes.
              </p>
              <table role="presentation" cellpadding="0" cellspacing="0" width="100%" style="margin:24px 0;">
                <tr>
                  <td align="center">
                    <a href="{{resetUrl}}" style="display:inline-block;background-color:#1a6cff;color:#ffffff;text-decoration:none;padding:12px 28px;border-radius:999px;font-weight:600;letter-spacing:0.3px;">
                      Reset password
                    </a>
                  </td>
                </tr>
              </table>
              <p style="margin:0 0 12px;line-height:1.6;">If the button doesn't work, copy and paste this link into your browser:</p>
              <p style="margin:0 0 24px;line-height:1.6;word-break:break-all;"><a href="{{resetUrl}}" style="color:#1a6cff;text-decoration:none;">{{resetUrl}}</a></p>
              <p style="margin:0 0 16px;line-height:1.6;">
                If you didn't request this password change, you can safely ignore this message or contact the DOS support team.
              </p>
              <p style="margin:0;line-height:1.6;">Best regards,<br />{{supportName}}</p>
            </td>
          </tr>
          <tr>
            <td style="padding:22px 28px;background-color:#f0f4fb;border-top:1px solid #d9e3f2;">
              <p style="margin:0;font-size:13px;color:#5f6b7a;line-height:1.6;">
                This email was sent to <strong>{{user.Email}}</strong> because it is linked to a DrinkOrder System account.
                If you no longer wish to receive account emails, please update your notification preferences.
              </p>
            </td>
          </tr>
        </table>
        <p style="margin:18px 0 0;font-size:12px;color:#8c97a4;line-height:1.6;">&copy; {{currentYear}} DrinkOrder System (DOS). All rights reserved.</p>
      </td>
    </tr>
  </table>
</div>
""";

            await _email.SendAsync(user.Email!, "Reset your DrinkOrder password", html);

            return token;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDTO dto)
        {
            var cacheKey = $"pwdreset:{dto.Token}";
            if (!_cache.TryGetValue(cacheKey, out int userId))
                return false;

            var user = await _repo.GetByIdAsync(userId);
            if (user == null) return false;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(user);

            _cache.Remove(cacheKey);
            return true;
        }
    }
}

