using PaymentAPI.DTOs;

namespace PaymentAPI.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentResponseDTO> CreatePaymentAsync(PaymentRequestDTO request);
        Task<IEnumerable<PaymentResultDTO>> GetAllAsync();
        Task<PaymentResultDTO?> GetByIdAsync(int id);
        Task<PaymentResultDTO> UpdateAsync(int id, PaymentUpdateDTO request);
        Task<PaymentDeleteResultDTO> DeleteAsync(int id);
        Task<PaymentResultDTO> ConfirmPaymentAsync(int paymentId, string status, string? transactionId = null);
        Task<PaymentResponseDTO> CreateVnPayPaymentAsync(PaymentRequestDTO request, string ipAddress);
        Task<IEnumerable<PaymentResultDTO>> GetByOrderIdAsync(int orderId);
    }
}
