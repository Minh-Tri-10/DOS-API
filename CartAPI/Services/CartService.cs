using CartAPI.Models;
using CartAPI.Repositories.Interfaces;
using CartAPI.Services.Interfaces;

namespace CartAPI.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _repo;
        public CartService(ICartRepository repo) => _repo = repo;

        public Task<Cart?> GetUserCartAsync(int userId) => _repo.GetCartByUserIdAsync(userId);
        public Task<Cart?> GetCartByIdAsync(int cartId) => _repo.GetCartByIdAsync(cartId);

        public async Task<Cart> GetOrCreateUserCartAsync(int userId)
        {
            var cart = await _repo.GetCartByUserIdAsync(userId);
            return cart ?? await _repo.CreateCartForUserAsync(userId);
        }

        public async Task AddItemToCartAsync(int cartId, int productId, int quantity)
        {
            if (quantity <= 0) quantity = 1;

            // ✅ nếu đã có -> tăng số lượng
            var exist = await _repo.GetCartItemAsync(cartId, productId);
            if (exist != null)
            {
                exist.Quantity += quantity;
                exist.UpdatedAt = DateTime.UtcNow;
                await _repo.UpdateCartItemAsync(exist);
                return;
            }

            // chưa có -> thêm mới
            var item = new CartItem
            {
                CartId = cartId,
                ProductId = productId,
                Quantity = quantity,
                CreatedAt = DateTime.UtcNow
            };
            await _repo.AddCartItemAsync(item);
        }

        public async Task UpdateItemQuantityAsync(int cartItemId, int quantity)
        {
            if (quantity <= 0)
            {
                // cho phép xoá khi về 0
                await _repo.RemoveCartItemAsync(cartItemId);
                return;
            }

            // ✅ lấy đúng CartItem theo Id rồi cập nhật
            var item = await _repo.GetCartItemByIdAsync(cartItemId);
            if (item == null) return;

            item.Quantity = quantity;
            item.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateCartItemAsync(item);
        }

        public Task RemoveItemFromCartAsync(int cartItemId) => _repo.RemoveCartItemAsync(cartItemId);
        public Task DeleteCartAsync(int cartId) => _repo.DeleteCartAsync(cartId);
    }
}