using OrderAPI.DTOs;
using OrderAPI.Services.Interfaces;

namespace OrderAPI.Services
{
    public class ProductClient : IProductClient
    {
        private readonly HttpClient _http;

        public ProductClient(HttpClient http) => _http = http;

        public async Task<ProductDto?> GetProductByIdAsync(int productId)
        {
            return await _http.GetFromJsonAsync<ProductDto>($"api/product/{productId}");
        }
    }
}
