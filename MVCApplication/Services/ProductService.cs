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

        public ProductService(IHttpClientFactory httpClientFactory, ILogger<ProductService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("CategoriesAPI");
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
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
    }
}

