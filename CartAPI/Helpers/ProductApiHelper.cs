using System.Net.Http.Json;

public static class ProductApiHelper
{
    private static readonly HttpClient _client = new HttpClient
    {
        BaseAddress = new Uri("https://localhost:7021/") // ✅ API Product của bạn
    };

    public static async Task<ProductDTO?> GetProductAsync(int productId)
    {
        return await _client.GetFromJsonAsync<ProductDTO>($"api/Product/{productId}");
    }
}

public class ProductDTO
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string ImageUrl { get; set; }
}
