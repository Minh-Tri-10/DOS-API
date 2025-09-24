using MVCApplication.DTOs;
using MVCApplication.Services.Interfaces;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace MVCApplication.Services
{
    public class ProductService : IProductService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public ProductService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7021/"); // Base root URL thôi

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<IEnumerable<ProductDTO>> GetAllAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<List<ProductDTO>>("api/Product", _jsonOptions);
            return response ?? new List<ProductDTO>();
        }

        public async Task<ProductDTO?> GetByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/Product/{id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ProductDTO>(json, _jsonOptions);
            }
            return null;
        }

        public async Task<ProductDTO> CreateAsync(CreateProductDTO dto, IFormFile? imageFile)
        {
            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(dto.ProductName ?? ""), "ProductName");
            formData.Add(new StringContent(dto.Description ?? ""), "Description");
            formData.Add(new StringContent(dto.Price.ToString()), "Price");
            formData.Add(new StringContent(dto.Stock?.ToString() ?? ""), "Stock");
            formData.Add(new StringContent(dto.CategoryId?.ToString() ?? ""), "CategoryId");
            if (imageFile != null)
            {
                var stream = imageFile.OpenReadStream();
                formData.Add(new StreamContent(stream), "ImageFile", imageFile.FileName);
            }
            var response = await _httpClient.PostAsync("api/Product", formData);
            response.EnsureSuccessStatusCode();
            var responseJson = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response content: {responseJson}"); // Log để kiểm tra
            return JsonSerializer.Deserialize<ProductDTO>(responseJson, _jsonOptions) ?? throw new Exception("Failed to create product.");
        }

        public async Task UpdateAsync(int id, UpdateProductDTO dto)
        {
            var json = JsonSerializer.Serialize(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"api/Product/{id}", content);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/Product/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}
