using CartAPI.Models;
using CartAPI.Repositories.Interfaces;
using CartAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CartAPI.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;

        public CartService(ICartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }

        public async Task<Cart?> GetUserCartAsync(int userId)
        {
            return await _cartRepository.GetCartByUserIdAsync(userId);
        }

        public async Task<Cart?> GetCartByIdAsync(int cartId)
        {
            return await _cartRepository.GetCartByIdAsync(cartId);
        }

        public async Task<Cart> CreateCartAsync(int userId)
        {
            var cart = new Cart
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _cartRepository.AddCartAsync(cart);
            return cart;
        }

        public async Task AddItemToCartAsync(int cartId, int productId, int quantity)
        {
            var cartItem = new CartItem
            {
                CartId = cartId,
                ProductId = productId,
                Quantity = quantity,
                CreatedAt = DateTime.UtcNow
            };

            await _cartRepository.AddCartItemAsync(cartItem);
        }

        public async Task UpdateItemQuantityAsync(int cartItemId, int quantity)
        {
            var cart = await _cartRepository.GetCartByIdAsync(cartItemId);
            if (cart != null)
            {
                var item = cart.CartItems.FirstOrDefault(i => i.CartItemId == cartItemId);
                if (item != null)
                {
                    item.Quantity = quantity;
                    item.UpdatedAt = DateTime.UtcNow;
                    await _cartRepository.UpdateCartItemAsync(item);
                }
            }
        }

        public async Task RemoveItemFromCartAsync(int cartItemId)
        {
            await _cartRepository.RemoveCartItemAsync(cartItemId);
        }

        public async Task DeleteCartAsync(int cartId)
        {
            await _cartRepository.DeleteCartAsync(cartId);
        }
        public async Task<Cart> GetOrCreateUserCartAsync(int userId)
        {
            var cart = await _cartRepository.GetCartByUserIdAsync(userId);
            if (cart != null) return cart;

            return await _cartRepository.CreateCartForUserAsync(userId);
        }

    }
}
