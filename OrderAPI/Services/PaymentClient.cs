using OrderAPI.DTOs;
using OrderAPI.Services.Interfaces;

namespace OrderAPI.Services
{
    public class PaymentClient : IPaymentClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PaymentClient> _logger;

        public PaymentClient(HttpClient httpClient, ILogger<PaymentClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<string?> GetPaymentStatusByOrderIdAsync(int orderId)
        {
            try
            {
                var url = $"https://localhost:7011/api/Payments/by-order/{orderId}";
                var payments = await _httpClient.GetFromJsonAsync<List<PaymentResultDTO>>(url);

                if (payments == null || payments.Count == 0)
                    return "Chưa thanh toán";

                var lastPayment = payments.LastOrDefault();
                return lastPayment?.PaymentStatus ?? "Không rõ";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gọi PaymentAPI cho OrderId={OrderId}", orderId);
                return "Không xác định";
            }
        }
    }
}
