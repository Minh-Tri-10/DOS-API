using System.Net.Http;
using System.Net.Http.Json;
using MVCApplication.Models;
using MVCApplication.Services.Interfaces;

namespace MVCApplication.Services
{
    public class CartService : ICartService
    {
        private readonly HttpClient _httpClient;

        public CartService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<Cart>> GetCartsAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<Cart>>("api/Cart");
        }

        public async Task<Cart> GetCartByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<Cart>($"api/Cart/{id}");
        }

        public async Task<Cart> CreateCartAsync(Cart cart)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Cart", cart);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Cart>();
        }

        public async Task<Cart> UpdateCartAsync(int id, Cart cart)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/Cart/{id}", cart);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Cart>();
        }

        public async Task<bool> DeleteCartAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/Cart/{id}");
            return response.IsSuccessStatusCode;
        }

        // =====================
        // Các API mở rộng
        // =====================

        public async Task<Cart> GetCartByUserIdAsync(int userId)
        {
            return await _httpClient.GetFromJsonAsync<Cart>($"api/Cart/user/{userId}");
        }

        public async Task<bool> AddToCartAsync(int userId, int productId, int quantity)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Cart/add", new { userId, productId, quantity });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> RemoveFromCartAsync(int cartItemId)
        {
            var response = await _httpClient.DeleteAsync($"api/Cart/remove/{cartItemId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ClearCartAsync(int userId)
        {
            var response = await _httpClient.DeleteAsync($"api/Cart/clear/{userId}");
            return response.IsSuccessStatusCode;
        }
    }
}
