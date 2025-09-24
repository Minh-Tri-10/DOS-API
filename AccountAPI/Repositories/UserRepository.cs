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
            // Lấy entry đang được track (nếu có)
            var entry = _ctx.Entry(user);

            if (entry.State == EntityState.Detached)
            {
                // Nếu entity chưa được track (Detached), attach rồi đánh dấu Modified
                _ctx.Attach(user);
                entry = _ctx.Entry(user);
                entry.State = EntityState.Modified; // mặc định: mọi property = Modified
            }
            else
            {
                // Nếu entity đã được truy xuất bằng cùng DbContext trước đó (Tracked),
                // EF đã tự so sánh thay đổi. Ta chỉ cần chặn các cột không cho ghi đè.
                entry.State = EntityState.Modified; // hợp nhất hành vi, rồi loại trừ từng cột
            }

            // KHÓA những cột không bao giờ ghi đè qua UpdateAsync chung:
            entry.Property(x => x.CreatedAt).IsModified = false;
            entry.Property(x => x.Username).IsModified = false;
            entry.Property(x => x.Role).IsModified = false;

            // Các cột khác (FullName, Email, Phone, AvatarUrl, PasswordHash, IsBanned, UpdatedAt)
            // vẫn để Modified theo trạng thái hiện tại

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
