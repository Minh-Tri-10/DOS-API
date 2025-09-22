using WebApp.Models;

namespace WebApp.Services.Interfaces
{
    public interface IAccountService
    {
        Task<UserViewModel?> LoginAsync(LoginViewModel dto);
        Task<UserViewModel?> RegisterAsync(RegisterViewModel dto);
        Task<UserViewModel?> GetByIdAsync(int id);
    }
}
