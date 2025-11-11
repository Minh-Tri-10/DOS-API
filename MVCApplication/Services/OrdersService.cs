using MVCApplication.DTOs;
using MVCApplication.Services.Interfaces;
using OrderAPI.DTOs;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using static MVCApplication.Services.OrdersService;
namespace MVCApplication.Services
{
    public class OrdersService : IOrderService
    {
        private readonly IAccountService _accountService;
        private readonly HttpClient _http;

        public OrdersService(HttpClient http, IAccountService accountService)
        {
            _http = http;
            _accountService = accountService;
        }

        public async Task<List<OrderDto>?> GetAllAsync()
        {
            return await _http.GetFromJsonAsync<List<OrderDto>>("api/order");
        }
        //public async Task<PagedResult<OrderDto>?> GetPagedAsync(int page = 1, int pageSize = 10)
        //{
        //    var response = await _http.GetAsync($"api/order?page={page}&pageSize={pageSize}");
        //    if (!response.IsSuccessStatusCode) return null;

        //    var result = await response.Content.ReadFromJsonAsync<PagedResult<OrderDto>>();
        //    return result;
        //}

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

        public async Task<PagedResult<OrderDto>> GetPagedAsync(
            int page = 1, int pageSize = 10,
            string? search = null, string? status = null)
        {
            int skip = (page - 1) * pageSize;
            var sb = new StringBuilder($"odata/Orders?$top={pageSize}&$skip={skip}&$count=true");

            var filters = new List<string>();
            // ❌ Không filter theo FullName nữa, vì FullName null khi API trả
            if (!string.IsNullOrWhiteSpace(status))
                filters.Add($"OrderStatus eq '{status}'");

            if (filters.Any())
                sb.Append("&$filter=" + string.Join(" and ", filters));

            sb.Append("&$orderby=OrderDate desc");

            var url = sb.ToString();
            var response = await _http.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"Lỗi gọi API: {response.StatusCode} - {content}");

            var odataResponse = JsonSerializer.Deserialize<ODataResponse<OrderDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var orders = odataResponse?.Value ?? new List<OrderDto>();
            int totalCount = odataResponse?.Count ?? orders.Count;

            // Map FullName từ AccountService
            if (orders.Any())
            {
                var userIds = orders.Select(o => o.UserId).Distinct().ToList();
                var userMap = new Dictionary<int, string>();

                foreach (var id in userIds)
                {
                    var user = await _accountService.GetByIdAsync(id);
                    userMap[id] = user?.FullName ?? "Unknown";
                }

                orders.ForEach(o => o.FullName = userMap.GetValueOrDefault(o.UserId, "Unknown"));
            }

            // Filter theo search tên ở đây, sau khi map xong
            if (!string.IsNullOrWhiteSpace(search))
            {
                var lowerSearch = search.Trim().ToLower();
                orders = orders
                    .Where(o => !string.IsNullOrEmpty(o.FullName) && o.FullName.ToLower().Contains(lowerSearch))
                    .ToList();
                totalCount = orders.Count; // cập nhật lại tổng số
            }

            return new PagedResult<OrderDto>
            {
                Data = orders,
                TotalCount = totalCount
            };
        }


        // OData response model
        public class ODataResponse<T>
        {
            [JsonPropertyName("@odata.count")]
            public int? Count { get; set; }

            [JsonPropertyName("value")]
            public List<T>? Value { get; set; }
        }

    }
}


