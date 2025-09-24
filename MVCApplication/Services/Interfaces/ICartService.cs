using MVCApplication.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MVCApplication.Services.Interfaces
{
    public interface ICartService
    {
        Task<IEnumerable<Cart>> GetCartsAsync();
        Task<Cart> GetCartByIdAsync(int id);
        Task<Cart> CreateCartAsync(Cart cart);
        Task<Cart> UpdateCartAsync(int id, Cart cart);
        Task<bool> DeleteCartAsync(int id);
        Task<Cart> GetCartByUserIdAsync(int userId);
        Task<bool> AddToCartAsync(int userId, int productId, int quantity);
        Task<bool> RemoveFromCartAsync(int cartItemId);
    }
}
