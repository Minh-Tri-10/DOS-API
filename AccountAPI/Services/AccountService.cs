using AccountAPI.DTOs;
using AccountAPI.DTOs.AccountAPI.DTOs;
using AccountAPI.Models;
using AccountAPI.Repositories.Interfaces;
using AccountAPI.Services.Interfaces;
using BCrypt.Net;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;

namespace AccountAPI.Services
{

    public class AccountService : IAccountService
    {
        private readonly IUserRepository _repo;
        private readonly IMemoryCache _cache; // giữ token reset tạm

        public AccountService(IUserRepository repo, IMemoryCache cache)
        {
            _repo = repo;
            _cache = cache;
        }

        public async Task<UserDTO?> LoginAsync(LoginDTO dto)
        {
            var user = await _repo.GetByUsernameAsync(dto.Username);
            if (user == null) return null;
            if ((user.IsBanned ?? false)) return null;

            // Nếu hash cũ/invalid → coi như sai mật khẩu, KHÔNG Verify để tránh nổ
            if (!IsBcryptHash(user.PasswordHash)) return null;

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return null;

            return ToDTO(user);
        }

        private static bool IsBcryptHash(string? hash) =>
    !string.IsNullOrWhiteSpace(hash) &&
    (hash!.StartsWith("$2a$") || hash.StartsWith("$2b$") || hash.StartsWith("$2y$"));

        public async Task<UserDTO> RegisterAsync(RegisterDTO dto)
        {
            if (await _repo.GetByUsernameAsync(dto.Username) != null)
                throw new Exception("Username already exists");

            var user = new User
            {
                Username = dto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                FullName = dto.FullName,
                Email = dto.Email,
                Role = "customer",
                CreatedAt = DateTime.UtcNow
                // IsBanned là null => coi như false
            };

            await _repo.AddAsync(user);
            return ToDTO(user);
        }

        public async Task<IEnumerable<UserDTO>> GetAllAsync() =>
            (await _repo.GetAllAsync()).Select(ToDTO);

        public async Task<UserDTO?> GetByIdAsync(int id)
        {
            var u = await _repo.GetByIdAsync(id);
            return u == null ? null : ToDTO(u);
        }

        public async Task<UserDTO?> UpdateProfileAsync(int userId, UpdateProfileDTO dto)
        {
            var u = await _repo.GetByIdAsync(userId);
            if (u == null) return null;

            u.FullName = dto.FullName ?? u.FullName;
            u.Email = dto.Email ?? u.Email;
            u.Phone = dto.Phone ?? u.Phone;
            u.AvatarUrl = dto.AvatarUrl ?? u.AvatarUrl;
            u.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(u);
            return ToDTO(u);
        }

        public async Task<bool> ChangePasswordAsync(ChangePasswordDTO dto)
        {
            var u = await _repo.GetByIdAsync(dto.UserId);
            if (u == null) return false;

            // Nếu hash cũ/invalid thì yêu cầu user đăng nhập lại/đặt lại mật khẩu
            if (!IsBcryptHash(u.PasswordHash)) return false;

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, u.PasswordHash))
                return false;

            u.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            u.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(u);
            return true;
        }


        public async Task<bool> SetBanAsync(int userId, bool isBanned)
        {
            var u = await _repo.GetByIdAsync(userId);
            if (u == null) return false;
            u.IsBanned = isBanned; // bool? trong model
            u.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(u);
            return true;
        }

        // ===== Forgot/Reset password bằng MemoryCache (đơn giản để test) =====
        // Cache key: "pwdreset:{token}" -> userId
        public async Task<string?> ForgotPasswordAsync(ForgotPasswordDTO dto)
        {
            var u = await _repo.GetByEmailAsync(dto.Email);
            if (u == null) return null; // tránh lộ user

            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(24));
            var cacheKey = $"pwdreset:{token}";

            _cache.Set(cacheKey, u.UserId, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
            });

            // TODO: gửi email kèm token (hoặc link FE /reset?token=...)
            return token; // trả về để bạn test nhanh trên Swagger
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

        // helper map (giữ nguyên như trước)
        private static UserDTO ToDTO(User u) => new UserDTO
        {
            UserId = u.UserId,
            Username = u.Username,
            FullName = u.FullName,
            Email = u.Email,
            Role = u.Role,
            IsBanned = u.IsBanned ?? false,
            CreatedAt = u.CreatedAt
        };
    }
}

