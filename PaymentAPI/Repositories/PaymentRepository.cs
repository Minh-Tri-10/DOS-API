using Microsoft.EntityFrameworkCore;
using PaymentAPI.Models;
using PaymentAPI.Repositories.Interfaces;

namespace PaymentAPI.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly DrinkOrderDbContext _context;
        public PaymentRepository(DrinkOrderDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Payment>> GetAllAsync()
            => await _context.Payments.ToListAsync();

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
    }

}
