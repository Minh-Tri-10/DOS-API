using System.Collections.Generic;
using System.Threading.Tasks;
using AccountAPI.DTOs;

namespace AccountAPI.Services.Interfaces
{
    // Khai báo toàn bộ tác vụ nghiệp vụ mà tầng controller cần.
    public interface IAccountService
    {
        Task<AuthResponseDTO?> LoginAsync(LoginDTO dto);
        Task<UserDTO> RegisterAsync(RegisterDTO dto);
        Task<IEnumerable<UserDTO>> GetAllAsync();
        Task<UserDTO?> GetByIdAsync(int id);

        Task<UserDTO?> UpdateProfileAsync(int userId, UpdateProfileDTO dto, IFormFile? avatarFile);
        Task<bool> ChangePasswordAsync(ChangePasswordDTO dto);
        Task<bool> SetBanAsync(int userId, bool isBanned);

        Task<string?> ForgotPasswordAsync(ForgotPasswordDTO dto);
        Task<bool> ResetPasswordAsync(ResetPasswordDTO dto);
    }
}

