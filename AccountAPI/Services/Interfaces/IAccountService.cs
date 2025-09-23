using AccountAPI.DTOs;

namespace AccountAPI.Services.Interfaces
{
    public interface IAccountService
    {
        Task<UserDTO?> LoginAsync(LoginDTO dto);
        Task<UserDTO> RegisterAsync(RegisterDTO dto);
        Task<IEnumerable<UserDTO>> GetAllAsync();
        Task<UserDTO?> GetByIdAsync(int id);

        Task<UserDTO?> UpdateProfileAsync(int userId, UpdateProfileDTO dto); // NEW
        Task<bool> ChangePasswordAsync(ChangePasswordDTO dto);                // NEW
        Task<bool> SetBanAsync(int userId, bool isBanned);                    // NEW

        Task<string?> ForgotPasswordAsync(ForgotPasswordDTO dto);             // NEW (trả token test)
        Task<bool> ResetPasswordAsync(ResetPasswordDTO dto);                  // NEW
    }
}
