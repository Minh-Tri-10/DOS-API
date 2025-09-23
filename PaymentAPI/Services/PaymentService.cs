using AutoMapper;
using PaymentAPI.DTOs;
using PaymentAPI.Models;
using PaymentAPI.Repositories.Interfaces;
using PaymentAPI.Services.Interfaces;

namespace PaymentAPI.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _repo;
        private readonly IMapper _mapper;

        public PaymentService(IPaymentRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<PaymentResponseDTO> CreatePaymentAsync(PaymentRequestDTO request)
        {
            var entity = _mapper.Map<Payment>(request);
            entity.PaymentStatus = "pending";
            entity.PaymentDate = DateTime.UtcNow;

            await _repo.AddAsync(entity);

            return new PaymentResponseDTO
            {
                PaymentId = entity.PaymentId,
                PaidAmount = entity.PaidAmount,
                PaymentUrl = $"https://placeholder.local/pay/{entity.PaymentId}"
            };
        }

        public async Task<IEnumerable<PaymentResultDTO>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return _mapper.Map<IEnumerable<PaymentResultDTO>>(list);
        }

        public async Task<PaymentResultDTO?> GetByIdAsync(int id)
        {
            var p = await _repo.GetByIdAsync(id);
            return p == null ? null : _mapper.Map<PaymentResultDTO>(p);
        }

        public async Task<PaymentResultDTO> UpdateAsync(int id, PaymentUpdateDTO request)
        {
            var p = await _repo.GetByIdAsync(id);
            if (p == null) throw new KeyNotFoundException("Payment not found");

            _mapper.Map(request, p);
            p.PaymentDate = DateTime.UtcNow;
            await _repo.UpdateAsync(p);

            return _mapper.Map<PaymentResultDTO>(p);
        }

        public async Task<PaymentDeleteResultDTO> DeleteAsync(int id)
        {
            var p = await _repo.GetByIdAsync(id);
            if (p == null) throw new KeyNotFoundException("Payment not found");

            await _repo.DeleteAsync(p);
            return new PaymentDeleteResultDTO { Success = true, Message = $"Payment {id} deleted" };
        }

        public async Task<PaymentResultDTO> ConfirmPaymentAsync(int paymentId, string status, string? transactionId = null)
        {
            var p = await _repo.GetByIdAsync(paymentId);
            if (p == null) throw new KeyNotFoundException("Payment not found");

            p.PaymentStatus = status;
            p.PaymentDate = DateTime.UtcNow;
            // nếu muốn lưu transactionId, thêm field vào model/entity
            await _repo.UpdateAsync(p);
            return _mapper.Map<PaymentResultDTO>(p);
        }
    }

}
