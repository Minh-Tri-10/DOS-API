using AccountAPI.Models;
using AccountAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AccountAPI.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AccountDbContext _ctx;
        public UserRepository(AccountDbContext ctx) => _ctx = ctx;

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
            // L?y entry dang du?c track (n?u c�)
            var entry = _ctx.Entry(user);

            if (entry.State == EntityState.Detached)
            {
                // N?u entity chua du?c track (Detached), attach r?i d�nh d?u Modified
                _ctx.Attach(user);
                entry = _ctx.Entry(user);
                entry.State = EntityState.Modified; // m?c d?nh: m?i property = Modified
            }
            else
            {
                // N?u entity d� du?c truy xu?t b?ng c�ng DbContext tru?c d� (Tracked),
                // EF d� t? so s�nh thay d?i. Ta ch? c?n ch?n c�c c?t kh�ng cho ghi d�.
                entry.State = EntityState.Modified; // h?p nh?t h�nh vi, r?i lo?i tr? t?ng c?t
            }

            // KH�A nh?ng c?t kh�ng bao gi? ghi d� qua UpdateAsync chung:
            entry.Property(x => x.CreatedAt).IsModified = false;
            entry.Property(x => x.Username).IsModified = false;
            entry.Property(x => x.Role).IsModified = false;

            // C�c c?t kh�c (FullName, Email, Phone, AvatarUrl, PasswordHash, IsBanned, UpdatedAt)
            // v?n d? Modified theo tr?ng th�i hi?n t?i

            await _ctx.SaveChangesAsync();
        }
        public async Task UpdateProfileAsync(User user)
        {
            var entry = _ctx.Entry(user);
            if (entry.State == EntityState.Detached)
                _ctx.Attach(user);

            entry.Property(x => x.FullName).IsModified = true;
            entry.Property(x => x.Email).IsModified = true;
            entry.Property(x => x.Phone).IsModified = true;
            entry.Property(x => x.AvatarUrl).IsModified = true;
            entry.Property(x => x.UpdatedAt).IsModified = true;

            await _ctx.SaveChangesAsync();
        }

        public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
    }
}
