using AccountAPI.DTOs;
using AccountAPI.DTOs.AccountAPI.DTOs;
using AccountAPI.Models;
using AccountAPI.Repositories.Interfaces;
using AccountAPI.Services.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;

namespace AccountAPI.Services
{
    public class AccountService : IAccountService
    {
        private readonly IUserRepository _repo;
        private readonly IMemoryCache _cache;   // giữ token reset tạm
        private readonly IMapper _mapper;       // AutoMapper

        public AccountService(IUserRepository repo, IMemoryCache cache, IMapper mapper)
        {
            _repo = repo;
            _cache = cache;
            _mapper = mapper;
        }

        public async Task<UserDTO?> LoginAsync(LoginDTO dto)
        {
            var user = await _repo.GetByUsernameAsync(dto.Username);
            if (user == null) return null;
            if ((user.IsBanned ?? false)) return null;

            // Nếu hash cũ/invalid → coi như sai mật khẩu
            if (!IsBcryptHash(user.PasswordHash)) return null;

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return null;

            return _mapper.Map<UserDTO>(user);
        }

        private static bool IsBcryptHash(string? hash) =>
            !string.IsNullOrWhiteSpace(hash) &&
            (hash!.StartsWith("$2a$") || hash.StartsWith("$2b$") || hash.StartsWith("$2y$"));

        public async Task<UserDTO> RegisterAsync(RegisterDTO dto)
        {
            if (await _repo.GetByUsernameAsync(dto.Username) != null)
                throw new Exception("Username already exists");

            // map DTO -> User (các field Role, CreatedAt đã set trong MappingProfile)
            var user = _mapper.Map<User>(dto);
            // hash password riêng
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

            // chỉ map các field != null (đã cấu hình Condition trong MappingProfile)
            _mapper.Map(dto, u);
            u.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(u);
            return _mapper.Map<UserDTO>(u);
        }

        public async Task<bool> ChangePasswordAsync(ChangePasswordDTO dto)
        {
            var u = await _repo.GetByIdAsync(dto.UserId);
            if (u == null) return false;

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

            u.IsBanned = isBanned;
            u.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(u);
            return true;
        }

        // ===== Forgot/Reset password bằng MemoryCache =====
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

            // TODO: gửi email kèm token (prod)
            return token; // trả về để test nhanh
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
