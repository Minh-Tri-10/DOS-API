using MVCApplication.DTOs;
using MVCApplication.Services.Interfaces;
using OrderAPI.DTOs;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
namespace MVCApplication.Services
{
    public class OrdersService : IOrderService
    {
        private readonly HttpClient _http;
        public OrdersService(HttpClient http) => _http = http;

        public async Task<List<OrderDto>?> GetAllAsync()
        {
            return await _http.GetFromJsonAsync<List<OrderDto>>("api/order");
        }
        public async Task<PagedResult<OrderDto>?> GetPagedAsync(int page = 1, int pageSize = 10)
        {
            var response = await _http.GetAsync($"api/order?page={page}&pageSize={pageSize}");
            if (!response.IsSuccessStatusCode) return null;

            var result = await response.Content.ReadFromJsonAsync<PagedResult<OrderDto>>();
            return result;
        }

        public async Task<OrderDto?> GetByIdAsync(int id)
        {
            return await _http.GetFromJsonAsync<OrderDto>($"api/order/{id}");
        }

        public async Task<int?> CreateAsync(CreateOrderDto dto)
        {
            var res = await _http.PostAsJsonAsync("api/order", dto);
            if (!res.IsSuccessStatusCode) return null;
            if (res.Content.Headers.ContentLength == 0) return null; // kiểm tra rỗng

            // đọc Location header trả về CreatedAtAction
            var createdOrder = await res.Content.ReadFromJsonAsync<OrderDto>();
            return createdOrder?.OrderId;
        }

        public async Task<bool> UpdateAsync(int id, UpdateOrderDto dto)
        {
            var res = await _http.PutAsJsonAsync($"api/order/{id}", dto);
            return res.IsSuccessStatusCode;
        }
        public async Task<List<OrderDto>?> GetOrdersByUserIdAsync(int userId)
        {
            try
            {
                var response = await _http.GetAsync($"api/Order/user/{userId}");
                response.EnsureSuccessStatusCode(); // Kiểm tra mã trạng thái HTTP
                return await response.Content.ReadFromJsonAsync<List<OrderDto>>();
            }
            catch (HttpRequestException ex)
            {
                // Xử lý lỗi kết nối hoặc lỗi HTTP khác
                throw new ApplicationException("Có lỗi xảy ra khi gọi API.", ex);
            }
        }
        // hủy đơn
        public async Task<bool> CancelAsync(int id, string reason)
        {
            var res = await _http.PutAsJsonAsync($"api/order/{id}/cancel", reason);
            return res.IsSuccessStatusCode;
        }

        public async Task<bool> MarkAsPaidAsync(int orderId)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, $"api/order/{orderId}/pay");
            var response = await _http.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        public async Task<ProductUsageDto> CheckProductUsageAsync(int productId)
        {
            if (productId <= 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(productId), "Product id must be greater than zero.");
            }

            var response = await _http.GetAsync($"api/order/check-product-usage?productId={productId}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return new ProductUsageDto
                {
                    IsUsed = false,
                    OrderCount = 0
                };
            }

            response.EnsureSuccessStatusCode();

            var usage = await response.Content.ReadFromJsonAsync<ProductUsageDto>();
            return usage ?? new ProductUsageDto();
        }


    }
}


