using AccountAPI.Models;
using AccountAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AccountAPI.Repositories
{
    // Repository EF Core cho cac thao tac voi bang Users.
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
            // L?y entry dang du?c track (n?u có)
            var entry = _ctx.Entry(user);

            if (entry.State == EntityState.Detached)
            {
                // N?u entity chua du?c track (Detached), attach r?i dánh d?u Modified
                _ctx.Attach(user);
                entry = _ctx.Entry(user);
                entry.State = EntityState.Modified; // m?c d?nh: m?i property = Modified
            }
            else
            {
                // N?u entity dã du?c truy xu?t b?ng cùng DbContext tru?c dó (Tracked),
                // EF dã t? so sánh thay d?i. Ta ch? c?n ch?n các c?t không cho ghi dè.
                entry.State = EntityState.Modified; // h?p nh?t hành vi, r?i lo?i tr? t?ng c?t
            }

            // KHÓA nh?ng c?t không bao gi? ghi dè qua UpdateAsync chung:
            entry.Property(x => x.CreatedAt).IsModified = false;
            entry.Property(x => x.Username).IsModified = false;
            entry.Property(x => x.Role).IsModified = false;

            // Các c?t khác (FullName, Email, Phone, AvatarUrl, PasswordHash, IsBanned, UpdatedAt)
            // v?n d? Modified theo tr?ng thái hi?n t?i

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
