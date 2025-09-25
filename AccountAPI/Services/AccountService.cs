using AccountAPI.DTOs;
using AccountAPI.Models;
using AccountAPI.Repositories.Interfaces;
using AccountAPI.Services.Interfaces;
using AccountAPI.Services.Email;              // NEW: email sender
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;

namespace AccountAPI.Services
{
    public class AccountService : IAccountService
    {
        private readonly IUserRepository _repo;
        private readonly IMemoryCache _cache;
        private readonly IMapper _mapper;
        private readonly IEmailSender _email;     // NEW
        private readonly IConfiguration _cfg;     // NEW

        public AccountService(
            IUserRepository repo,
            IMemoryCache cache,
            IMapper mapper,
            IEmailSender email,                   // NEW
            IConfiguration cfg)                   // NEW
        {
            _repo = repo;
            _cache = cache;
            _mapper = mapper;
            _email = email;                       // NEW
            _cfg = cfg;                           // NEW
        }

        public async Task<UserDTO?> LoginAsync(LoginDTO dto)
        {
            var user = await _repo.GetByUsernameAsync(dto.Username);
            if (user == null) return null;
            if ((user.IsBanned ?? false)) return null;

            if (!IsBcryptHash(user.PasswordHash)) return null;
            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash)) return null;

            return _mapper.Map<UserDTO>(user);
        }

        private static bool IsBcryptHash(string? hash) =>
            !string.IsNullOrWhiteSpace(hash) &&
            (hash!.StartsWith("$2a$") || hash.StartsWith("$2b$") || hash.StartsWith("$2y$"));

        public async Task<UserDTO> RegisterAsync(RegisterDTO dto)
        {
            if (await _repo.GetByUsernameAsync(dto.Username) != null)
                throw new Exception("Username already exists");

            var user = _mapper.Map<User>(dto);
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            await _repo.AddAsync(user);
            return _mapper.Map<UserDTO>(user);
        }

        public async Task<IEnumerable<UserDTO>> GetAllAsync() =>
            (await _repo.GetAllAsync()).Select(u => _mapper.Map<UserDTO>(u));

        public async Task<UserDTO?> GetByIdAsync(int id)
        {
            var u = await _repo.GetByIdAsync(id);
            return u == null ? null : _mapper.Map<UserDTO>(u);
        }

        public async Task<UserDTO?> UpdateProfileAsync(int userId, UpdateProfileDTO dto)
        {
            var u = await _repo.GetByIdAsync(userId);
            if (u == null) return null;

            _mapper.Map(dto, u);
            u.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateProfileAsync(u);  // <- chuyên biệt
            return _mapper.Map<UserDTO>(u);
        }

        public async Task<bool> ChangePasswordAsync(ChangePasswordDTO dto)
        {
            var u = await _repo.GetByIdAsync(dto.UserId);
            if (u == null) return false;

            if (!IsBcryptHash(u.PasswordHash)) return false;
            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, u.PasswordHash)) return false;

            u.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            u.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(u);
            return true;
        }

        public async Task<bool> SetBanAsync(int userId, bool isBanned)
        {
            var u = await _repo.GetByIdAsync(userId);
            if (u == null) return false;

            u.IsBanned = isBanned;
            u.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(u);
            return true;
        }

        // ========== Forgot/Reset password (email + MemoryCache) ==========

        // Helper: tạo Base64URL token an toàn khi bỏ vào URL
        private static string ToBase64Url(byte[] bytes) =>
            Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');

        public async Task<string?> ForgotPasswordAsync(ForgotPasswordDTO dto)
        {
            var u = await _repo.GetByEmailAsync(dto.Email);
            if (u == null) return null; // tránh lộ user

            // Tạo token URL-safe
            var token = ToBase64Url(RandomNumberGenerator.GetBytes(24));
            var cacheKey = $"pwdreset:{token}";

            _cache.Set(cacheKey, u.UserId, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
            });

            // Link FE đọc từ cấu hình: Frontend:BaseUrl (ví dụ https://localhost:7223)
            var feBase = _cfg["Frontend:BaseUrl"]?.TrimEnd('/') ?? "https://localhost:7223";
            var resetUrl = $"{feBase}/Accounts/ResetPassword?token={Uri.EscapeDataString(token)}";

            var html = $@"
                <p>Xin chào {u.FullName ?? u.Username},</p>
                <p>Bạn vừa yêu cầu đặt lại mật khẩu cho tài khoản DrinkOrder.</p>
                <p>Nhấn liên kết dưới đây trong 15 phút để đặt lại mật khẩu:</p>
                <p><a href=""{resetUrl}"">{resetUrl}</a></p>
                <p>Nếu không phải bạn, vui lòng bỏ qua email này.</p>";

            // Gửi email thật (SMTP)
            await _email.SendAsync(u.Email!, "Đặt lại mật khẩu - DrinkOrder", html);

            // DEV tiện test: vẫn trả token (controller có thể trả 204 để không lộ email)
            return token;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDTO dto)
        {
            var cacheKey = $"pwdreset:{dto.Token}";
            if (!_cache.TryGetValue(cacheKey, out int userId))
                return false;

            var u = await _repo.GetByIdAsync(userId);
            if (u == null) return false;

            u.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            u.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(u);

            _cache.Remove(cacheKey);
            return true;
        }
    }
}
