using CartAPI.Models;

namespace CartAPI.Services.Interfaces
{
    public interface ICartService
    {
        Task<Cart?> GetUserCartAsync(int userId);
        Task<Cart?> GetCartByIdAsync(int cartId);
        Task<Cart> CreateCartAsync(int userId);
        Task AddItemToCartAsync(int cartId, int productId, int quantity);
        Task UpdateItemQuantityAsync(int cartItemId, int quantity);
        Task RemoveItemFromCartAsync(int cartItemId);
        Task DeleteCartAsync(int cartId);
    }
}
