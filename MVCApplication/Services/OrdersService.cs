using MVCApplication.DTOs;
using MVCApplication.Services.Interfaces;
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

        public async Task<OrderDto?> GetByIdAsync(int id)
        {
            return await _http.GetFromJsonAsync<OrderDto>($"api/order/{id}");
        }

        public async Task<int?> CreateAsync(CreateOrderDto dto)
        {
            var res = await _http.PostAsJsonAsync("api/order", dto);
            if (!res.IsSuccessStatusCode) return null;

            // đọc Location header trả về CreatedAtAction
            var createdOrder = await res.Content.ReadFromJsonAsync<OrderDto>();
            return createdOrder?.OrderId;
        }

        public async Task<bool> UpdateAsync(int id, UpdateOrderDto dto)
        {
            var res = await _http.PutAsJsonAsync($"api/order/{id}", dto);
            return res.IsSuccessStatusCode;
        }
    }
}
