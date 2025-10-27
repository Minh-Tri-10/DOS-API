using MVCApplication.Models;

namespace MVCApplication.Services.Interfaces
{
    public interface IAccountService
    {
        Task<AuthResponseViewModel?> LoginAsync(LoginViewModel dto);
        Task<UserViewModel?> RegisterAsync(RegisterViewModel dto);
        Task<UserViewModel?> GetByIdAsync(int id);
        Task<IEnumerable<UserViewModel>> GetAllAsync();
        Task<bool> SetBanAsync(int id, bool isBanned);

        // NEW ↓↓↓
        Task<bool> ForgotPasswordAsync(string email);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
        Task<UserViewModel?> UpdateProfileAsync(int id, UpdateProfileViewModel dto);

    }
}
