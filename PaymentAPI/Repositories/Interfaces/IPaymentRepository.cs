using PaymentAPI.Models;

namespace PaymentAPI.Repositories.Interfaces
{
    public interface IPaymentRepository
    {
        Task<IEnumerable<Payment>> GetAllAsync();
        Task<Payment?> GetByIdAsync(int id);
        Task AddAsync(Payment payment);
        Task UpdateAsync(Payment payment);
        Task DeleteAsync(Payment payment);

    }
}
