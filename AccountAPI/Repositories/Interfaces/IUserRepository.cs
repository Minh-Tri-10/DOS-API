using AccountAPI.Models;

namespace AccountAPI.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllAsync();
        Task AddAsync(User user);
        Task UpdateAsync(User user); // NEW
        Task SaveChangesAsync();     // optional helper
        Task UpdateProfileAsync(User user);
    }
}
