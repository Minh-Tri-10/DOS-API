using OrderAPI.DTOs;

namespace OrderAPI.Services.Interfaces
{
    public interface ICatalogProductClient
    {
        Task<ProductDto?> GetProductByIdAsync(int productId);
    }
}
