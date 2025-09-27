using OrderAPI.DTOs;

namespace OrderAPI.Services.Interfaces
{
    public interface IProductClient
    {
        Task<ProductDto?> GetProductByIdAsync(int productId);
    }
}
