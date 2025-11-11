using MVCApplication.DTOs;
using MVCApplication.Services.Interfaces;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace MVCApplication.Services
{
    public class ProductService : IProductService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProductService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ProductService(IHttpClientFactory httpClientFactory, ILogger<ProductService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClientFactory.CreateClient("CategoriesAPI");
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<ProductDTO>> GetAllAsync()
        {
            var response = await _httpClient.GetAsync("api/Product");
            if (!response.IsSuccessStatusCode)
            {
                var payload = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("GET api/Product failed with {StatusCode}. Body: {Body}", (int)response.StatusCode, payload);
                response.EnsureSuccessStatusCode();
            }

            var products = await response.Content.ReadFromJsonAsync<List<ProductDTO>>(_jsonOptions);
            return products ?? new List<ProductDTO>();
        }

        public async Task<ProductDTO?> GetByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/Product/{id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ProductDTO>(json, _jsonOptions);
            }
            else
            {
                var payload = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("GET api/Product/{Id} failed with {StatusCode}. Body: {Body}", id, (int)response.StatusCode, payload);
            }
            return null;
        }

        public async Task<ProductDTO> CreateAsync(CreateProductDTO dto, IFormFile? imageFile)
        {
            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(dto.ProductName ?? string.Empty), "ProductName");
            formData.Add(new StringContent(dto.Description ?? string.Empty), "Description");
            formData.Add(new StringContent(dto.Price.ToString()), "Price");
            formData.Add(new StringContent(dto.Stock?.ToString() ?? string.Empty), "Stock");
            formData.Add(new StringContent(dto.CategoryId?.ToString() ?? string.Empty), "CategoryId");
            if (imageFile != null)
            {
                var stream = imageFile.OpenReadStream();
                formData.Add(new StreamContent(stream), "ImageFile", imageFile.FileName);
            }
            var response = await _httpClient.PostAsync("api/Product", formData);
            if (!response.IsSuccessStatusCode)
            {
                var payload = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("POST api/Product failed with {StatusCode}. Body: {Body}", (int)response.StatusCode, payload);
                response.EnsureSuccessStatusCode();
            }
            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ProductDTO>(responseJson, _jsonOptions) ?? throw new Exception("Failed to create product.");
        }

        public async Task UpdateAsync(int id, UpdateProductDTO dto)
        {
            var json = JsonSerializer.Serialize(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"api/Product/{id}", content);
            if (!response.IsSuccessStatusCode)
            {
                var payload = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("PUT api/Product/{Id} failed with {StatusCode}. Body: {Body}", id, (int)response.StatusCode, payload);
                response.EnsureSuccessStatusCode();
            }
        }

        public async Task DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/Product/{id}");
            if (!response.IsSuccessStatusCode)
            {
                var payload = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("DELETE api/Product/{Id} failed with {StatusCode}. Body: {Body}", id, (int)response.StatusCode, payload);
                response.EnsureSuccessStatusCode();
            }

        }
        // Hàm trừ hàng (Reduce Stock)
        public async Task<bool> ReduceStockAsync(int productId, int quantity)
        {
            var token = _httpContextAccessor.HttpContext?.User.FindFirst("access_token")?.Value;
            if (string.IsNullOrEmpty(token))
            {
                throw new InvalidOperationException("Không tìm thấy access token. Vui lòng đăng nhập lại.");
            }

            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var dto = new { ProductId = productId, Quantity = quantity };

            var response = await _httpClient.PutAsJsonAsync("api/Product/reduce-stock", dto);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"ReduceStock Error: {err}");
                return false;
            }

            return true;
        }
        public async Task<(IEnumerable<ProductDTO> Items, int TotalCount)> GetODataAsync(int page, int pageSize, string search, string orderBy, int? categoryId = null)
        {
            // Lấy token từ HttpContext.User.Claims (key "access_token" như trong controller)
            var token = _httpContextAccessor.HttpContext?.User.FindFirst("access_token")?.Value;
            if (string.IsNullOrEmpty(token))
            {
                throw new InvalidOperationException("Không tìm thấy access token. Vui lòng đăng nhập lại.");
            }
            // Set header Authorization
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Build query
            var skip = (page - 1) * pageSize;
            var filterParts = new List<string>();

            // Filter theo search (ProductName)
            if (!string.IsNullOrWhiteSpace(search))
            {
                filterParts.Add($"contains(ProductName,'{Uri.EscapeDataString(search)}')");
            }

            // Filter theo categoryId nếu có
            if (categoryId.HasValue)
            {
                filterParts.Add($"CategoryId eq {categoryId.Value}");
            }

            var filter = string.Join(" and ", filterParts);
            var queryParts = new List<string>();

            if (!string.IsNullOrEmpty(filter))
            {
                queryParts.Add($"$filter={filter}");
            }

            queryParts.Add($"$orderby={orderBy.Replace(" ", "%20")}"); // Encode space trong orderBy để tránh lỗi URL
            queryParts.Add($"$top={pageSize}");
            queryParts.Add($"$skip={skip}");
            queryParts.Add("$count=true");

            var query = $"odata/Products?{string.Join("&", queryParts)}";

            // Log query để debug
            Console.WriteLine("OData Query: " + query); // Hoặc dùng ILogger nếu có

            var response = await _httpClient.GetAsync(query);
            var content = await response.Content.ReadAsStringAsync();

            // Log content trước deserialize
            Console.WriteLine("OData Response Content: " + content);
            Console.WriteLine("Response Status: " + response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Lỗi gọi API: {response.StatusCode} - {content}");
            }

            // Deserialize với options linh hoạt hơn
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true, // Cho phép dư dấu phẩy
                ReadCommentHandling = JsonCommentHandling.Skip // Bỏ qua comment nếu có
            };

            try
            {
                var odataResponse = JsonSerializer.Deserialize<ODataProductRespone>(content, _jsonOptions);
                return (odataResponse?.Value ?? Enumerable.Empty<ProductDTO>(), odataResponse?.Count ?? 0);
            }
            catch
            {
                // Fallback: Deserialize trực tiếp thành mảng
                var items = JsonSerializer.Deserialize<IEnumerable<ProductDTO>>(content, _jsonOptions) ?? Enumerable.Empty<ProductDTO>();
                return (items, items.Count());
            }
        }
    }
}

