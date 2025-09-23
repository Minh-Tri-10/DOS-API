using MVCApplication.Models;

namespace MVCApplication.Services.Interfaces
{
    public interface IAccountService
    {
        Task<UserViewModel?> LoginAsync(LoginViewModel dto);
        Task<UserViewModel?> RegisterAsync(RegisterViewModel dto);
        Task<UserViewModel?> GetByIdAsync(int id);
        Task<IEnumerable<UserViewModel>> GetAllAsync();
        Task<bool> SetBanAsync(int id, bool isBanned);   // <-- thêm dòng này

    }

}
