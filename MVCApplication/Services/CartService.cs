using MVCApplication.DTOs;
using MVCApplication.Models;
using MVCApplication.Services.Interfaces;
using System.Net.Http.Json;

namespace MVCApplication.Services
{
    public class CartService : ICartService
    {
        private readonly HttpClient _cartApi;
        private readonly HttpClient _productApi;

        public CartService(IHttpClientFactory factory)
        {
            _cartApi = factory.CreateClient("CartAPI");
            _productApi = factory.CreateClient("ProductAPI"); // base 7021
        }

        public async Task<CartViewModel?> GetCartWithProductsAsync(int userId)
        {
            var cart = await _cartApi.GetFromJsonAsync<CartDTO>($"api/Cart/user/{userId}");
            if (cart == null) return null;

            var vm = new CartViewModel { CartId = cart.CartId, UserId = cart.UserId };

            foreach (var item in cart.CartItems)
            {
                // Product controller nằm ở CategoriesAPI: /api/Product/{id}
                var product = await _productApi.GetFromJsonAsync<ProductDTO>($"api/Product/{item.ProductId}");

                vm.CartItems.Add(new CartItemViewModel
                {
                    CartItemId = item.CartItemId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    ProductName = product?.ProductName ?? "N/A",
                    ImageUrl = product?.ImageUrl ?? "",
                    Price = product?.Price ?? 0
                });
            }
            return vm;
        }

        public async Task<bool> AddToCartAsync(int userId, int productId, int quantity)
        {
            // CartAPI.Add nhận JSON body: { userId, productId, quantity }
            var res = await _cartApi.PostAsJsonAsync("api/Cart/add", new { userId, productId, quantity });
            return res.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateQuantityAsync(int cartItemId, int quantity)
        {
            var res = await _cartApi.PutAsync($"api/Cart/update/{cartItemId}?quantity={quantity}", null);
            return res.IsSuccessStatusCode;
        }

        public async Task<bool> RemoveFromCartAsync(int cartItemId)
        {
            var res = await _cartApi.DeleteAsync($"api/Cart/remove/{cartItemId}");
            return res.IsSuccessStatusCode;
        }
    }
}
