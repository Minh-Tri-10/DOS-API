using AccountAPI.Models;
using AccountAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AccountAPI.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DrinkOrderContext _ctx;
        public UserRepository(DrinkOrderContext ctx) => _ctx = ctx;

        public Task<User?> GetByUsernameAsync(string username) =>
            _ctx.Users.FirstOrDefaultAsync(u => u.Username == username);

        public Task<User?> GetByIdAsync(int id) =>
            _ctx.Users.FirstOrDefaultAsync(u => u.UserId == id);

        public Task<User?> GetByEmailAsync(string email) =>
            _ctx.Users.FirstOrDefaultAsync(u => u.Email == email);

        public async Task<IEnumerable<User>> GetAllAsync() =>
            await _ctx.Users.AsNoTracking().ToListAsync();

        public async Task AddAsync(User user)
        {
            _ctx.Users.Add(user);
            await _ctx.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            _ctx.Users.Update(user);
            await _ctx.SaveChangesAsync();
        }

        public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
    }
}
