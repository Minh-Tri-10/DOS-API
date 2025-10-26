using System.Collections.Generic;
using System.Threading.Tasks;
using AccountAPI.DTOs;

namespace AccountAPI.Services.Interfaces
{
    public interface IAccountService
    {
        Task<UserDTO?> LoginAsync(LoginDTO dto);
        Task<UserDTO> RegisterAsync(RegisterDTO dto);
        Task<IEnumerable<UserDTO>> GetAllAsync();
        Task<UserDTO?> GetByIdAsync(int id);

        Task<UserDTO?> UpdateProfileAsync(int userId, UpdateProfileDTO dto);
        Task<bool> ChangePasswordAsync(ChangePasswordDTO dto);
        Task<bool> SetBanAsync(int userId, bool isBanned);

        Task<string?> ForgotPasswordAsync(ForgotPasswordDTO dto);
        Task<bool> ResetPasswordAsync(ResetPasswordDTO dto);
    }
}

