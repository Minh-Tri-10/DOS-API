using CartAPI.Helpers;
using CartAPI.Models;
using CartAPI.Repositories.Interfaces;
using CartAPI.Services.Interfaces;

namespace CartAPI.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _repo;
        private readonly ProductApiClient _productApi;

        public CartService(ICartRepository repo, ProductApiClient productApi)
        {
            _repo = repo;
            _productApi = productApi;
        }

        public Task<Cart?> GetUserCartAsync(int userId) => _repo.GetCartByUserIdAsync(userId);
        public Task<Cart?> GetCartByIdAsync(int cartId) => _repo.GetCartByIdAsync(cartId);

        public async Task<Cart> GetOrCreateUserCartAsync(int userId)
        {
            var cart = await _repo.GetCartByUserIdAsync(userId) ?? await _repo.CreateCartForUserAsync(userId);

            // ✅ Kiểm tra và xóa sản phẩm hết hàng
            if (cart.CartItems != null && cart.CartItems.Any())
            {
                var itemsToRemove = new List<int>();

                foreach (var item in cart.CartItems.ToList())
                {
                    var product = await _productApi.GetProductByIdAsync(item.ProductId);
                    if (product == null || product.Stock <= 0)
                    {
                        itemsToRemove.Add(item.CartItemId);
                    }
                }

                if (itemsToRemove.Count > 0)
                {
                    foreach (var id in itemsToRemove)
                    {
                        await _repo.RemoveCartItemAsync(id);
                    }

                    await _repo.SaveChangesAsync();

                    // Sau khi xóa xong, load lại cart sạch sẽ
                    cart = await _repo.GetCartByUserIdAsync(userId);
                }
            }

            return cart;
        }

        public async Task AddItemToCartAsync(int cartId, int productId, int quantity)
        {
            if (quantity <= 0) quantity = 1;

            // ✅ Gọi sang ProductAPI để kiểm tra thông tin
            var product = await _productApi.GetProductByIdAsync(productId);
            if (product == null)
                throw new Exception("Không thể lấy thông tin sản phẩm từ ProductAPI.");
            if (product.Stock <= 0)
                throw new Exception("Sản phẩm đã hết hàng!");

            var existing = await _repo.GetCartItemAsync(cartId, productId);
            if (existing != null)
            {
                existing.Quantity = Math.Min(existing.Quantity + quantity, product.Stock);
                existing.UpdatedAt = DateTime.UtcNow;
                await _repo.UpdateCartItemAsync(existing);
                return;
            }

            var newItem = new CartItem
            {
                CartId = cartId,
                ProductId = product.ProductId,
                Quantity = Math.Min(quantity, product.Stock),
                CreatedAt = DateTime.UtcNow
            };
            await _repo.AddCartItemAsync(newItem);
        }

        public async Task UpdateItemQuantityAsync(int cartItemId, int quantity)
        {
            var item = await _repo.GetCartItemByIdAsync(cartItemId);
            if (item == null) return;

            var product = await _productApi.GetProductByIdAsync(item.ProductId);
            if (product == null) return;

            item.Quantity = Math.Min(quantity, product.Stock);
            item.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateCartItemAsync(item);
        }

        public Task RemoveItemFromCartAsync(int cartItemId) => _repo.RemoveCartItemAsync(cartItemId);
        public Task DeleteCartAsync(int cartId) => _repo.DeleteCartAsync(cartId);

        // ✅ Thêm hàm này để implement interface
        public async Task SaveChangesAsync()
        {
            await _repo.SaveChangesAsync();
        }
    }
}
