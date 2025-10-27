using OrderAPI.DTOs;
using OrderAPI.Services.Interfaces;

namespace OrderAPI.Services
{
    public class CatalogProductClient : ICatalogProductClient
    {
        private readonly HttpClient _http;

        public CatalogProductClient(HttpClient http) => _http = http;

        public async Task<ProductDto?> GetProductByIdAsync(int productId)
        {
            return await _http.GetFromJsonAsync<ProductDto>($"api/product/{productId}");
        }
    }
}

