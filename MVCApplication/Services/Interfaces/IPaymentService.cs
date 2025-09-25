using MVCApplication.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MVCApplication.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<IEnumerable<PaymentResponseDTO>> GetAllAsync();
        Task<PaymentResponseDTO> GetByIdAsync(int id);
        Task CreateAsync(PaymentRequestDTO dto);
        Task UpdateAsync(int id, PaymentRequestDTO dto);
        Task DeleteAsync(int id);
    }
}
