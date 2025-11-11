using System.Net.Http.Json;

namespace CartAPI.Helpers
{
    public class ProductApiClient
    {
        private readonly HttpClient _client;

        public ProductApiClient(IHttpClientFactory factory)
        {
            _client = factory.CreateClient("ProductAPI");
        }

        public async Task<ProductContract?> GetProductByIdAsync(int productId)
        {
            try
            {
                return await _client.GetFromJsonAsync<ProductContract>($"api/Product/{productId}");
            }
            catch
            {
                return null;
            }
        }
    }

    // 🔹 Đây KHÔNG PHẢI là DTO của ProductAPI
    // mà chỉ là local contract để map JSON trả về
    public class ProductContract
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }
}
