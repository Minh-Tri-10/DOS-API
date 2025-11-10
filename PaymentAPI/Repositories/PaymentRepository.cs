using Microsoft.EntityFrameworkCore;
using PaymentAPI.Models;
using PaymentAPI.Repositories.Interfaces;

namespace PaymentAPI.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly PaymentDbContext _context;
        public PaymentRepository(PaymentDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Payment>> GetAllAsync()
            => await _context.Payments
                             .OrderByDescending(p => p.PaymentId)   // hoặc theo cột ngài muốn
                             .ToListAsync();

        public async Task<Payment?> GetByIdAsync(int id)
            => await _context.Payments.FindAsync(id);

        public async Task AddAsync(Payment payment)
        {
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Payment payment)
        {
            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Payment payment)
        {
            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<Payment>> GetByOrderIdAsync(int orderId)
        {
            return await _context.Payments
                .Where(p => p.OrderId == orderId)
                .OrderByDescending(p => p.PaymentId) // nếu muốn mới nhất trước
                .ToListAsync();
        }
    }

}
