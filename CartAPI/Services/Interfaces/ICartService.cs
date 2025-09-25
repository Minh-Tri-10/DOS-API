
using CartAPI.Models;

namespace CartAPI.Services.Interfaces
{
    public interface ICartService
    {
        Task<Cart?> GetUserCartAsync(int userId);
        Task<Cart?> GetCartByIdAsync(int cartId);

        Task<Cart> GetOrCreateUserCartAsync(int userId);

        // Thêm: cộng dồn nếu đã có
        Task AddItemToCartAsync(int cartId, int productId, int quantity);

        // Cập nhật đúng theo cartItemId
        Task UpdateItemQuantityAsync(int cartItemId, int quantity);

        Task RemoveItemFromCartAsync(int cartItemId);
        Task DeleteCartAsync(int cartId);
    }
}
