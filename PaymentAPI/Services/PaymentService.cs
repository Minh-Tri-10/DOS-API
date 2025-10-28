using AutoMapper;
using PaymentAPI.DTOs;
using PaymentAPI.Models;
using PaymentAPI.Repositories.Interfaces;
using PaymentAPI.Services.Interfaces;
using PaymentAPI.Utils;

namespace PaymentAPI.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _repo;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public PaymentService(IPaymentRepository repo, IMapper mapper, IConfiguration configuration)
        {
            _repo = repo;
            _mapper = mapper;
            _configuration = configuration;
        }

        public async Task<PaymentResponseDTO> CreatePaymentAsync(PaymentRequestDTO request)
        {
            var entity = _mapper.Map<Payment>(request);
            entity.PaymentStatus = "Pending";
            entity.PaymentDate = DateTime.UtcNow.AddHours(7);

            await _repo.AddAsync(entity);

            // Nếu là VNPay thì có PaymentUrl, còn COD thì để null
            string? paymentUrl = null;
            if (request.PaymentMethod?.Equals("VNPAY", StringComparison.OrdinalIgnoreCase) == true)
            {
                paymentUrl = $"https://placeholder.local/pay/{entity.PaymentId}";
            }

            return new PaymentResponseDTO
            {
                PaymentId = entity.PaymentId,
                PaidAmount = entity.PaidAmount,
                PaymentUrl = paymentUrl
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

        public async Task<PaymentResponseDTO> CreateVnPayPaymentAsync(PaymentRequestDTO request, string ipAddress)
        {
            // 1) tạo payment record Pending
            var payment = new Payment
            {
                OrderId = request.OrderId,
                PaidAmount = request.PaidAmount,
                PaymentMethod = "VNPAY",
                PaymentStatus = "Pending",
                PaymentDate = null  
            };
            await _repo.AddAsync(payment); // repo phải SaveChanges => payment.PaymentId được set

            // 2) build vnp params
            var cfg = _configuration.GetSection("Vnpay");
            var vnpUrl = cfg["Url"];
            var vnpTmn = cfg["TmnCode"];
            var vnpHash = cfg["HashSecret"];
            var returnUrl = cfg["ReturnUrl"];
            var ipnUrl = cfg["IpnUrl"];
            var version = cfg["Version"] ?? "2.1.0";

            // create unique txnRef -> dùng PaymentId (hoặc OrderId + timestamp)
            var txnRef = payment.PaymentId.ToString();

            var createDate = DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmss"); // GMT+7
            var expireDate = DateTime.UtcNow.AddHours(7).AddMinutes(int.Parse(cfg["ExpireMinutes"] ?? "15"))
                                .ToString("yyyyMMddHHmmss");

            var vnpInput = new Dictionary<string, string>
    {
        {"vnp_Version", version},
        {"vnp_Command", cfg["Command"] ?? "pay"},
        {"vnp_TmnCode", vnpTmn},
        {"vnp_Amount", ((long)(request.PaidAmount * 100)).ToString()}, // *100
        {"vnp_CurrCode", cfg["Currency"] ?? "VND"},
        {"vnp_TxnRef", txnRef},
        {"vnp_OrderInfo", $"Payment for order {request.OrderId}"},
        {"vnp_OrderType", "other"},
        {"vnp_Locale", cfg["Locale"] ?? "vn"},
        {"vnp_ReturnUrl", returnUrl},
        {"vnp_IpAddr", ipAddress ?? "0.0.0.0"},
        {"vnp_CreateDate", createDate},
        {"vnp_ExpireDate", expireDate},
        // nếu muốn cho khách chọn bank -> thêm vnp_BankCode
        // {"vnp_BankCode", "VNBANK"}
    };

            var paymentUrl = VnPayHelper.CreateRequestUrl(vnpUrl, vnpHash, vnpInput);

            return new PaymentResponseDTO
            {
                PaymentId = payment.PaymentId,
                PaidAmount = payment.PaidAmount,
                PaymentUrl = paymentUrl
            };
        }
        public async Task<IEnumerable<PaymentResultDTO>> GetByOrderIdAsync(int orderId)
        {
            var payments = await _repo.GetByOrderIdAsync(orderId);
            return _mapper.Map<IEnumerable<PaymentResultDTO>>(payments);
        }
    }

}
