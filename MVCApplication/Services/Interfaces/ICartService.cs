using MVCApplication.DTOs;
using MVCApplication.Models;


namespace MVCApplication.Services.Interfaces
{
    public interface ICartService
    {
        Task<CartViewModel?> GetCartWithProductsAsync(int userId);
        Task<bool> AddToCartAsync(int userId, int productId, int quantity);
        Task<bool> RemoveFromCartAsync(int cartItemId);
        Task<bool> UpdateQuantityAsync(int cartItemId, int quantity);

    }
}
