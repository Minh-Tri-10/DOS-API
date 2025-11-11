using CategoriesAPI.DTOs;
namespace CategoriesAPI.Services.Interfaces;
public interface IProductService
{
    Task<IEnumerable<ProductDTO>> GetAllAsync();
    IQueryable<ProductDTO> GetAllQueryable();
    Task<ProductDTO?> GetByIdAsync(int id);
    Task<ProductDTO> CreateAsync(CreateProductDTO dto);
    Task UpdateAsync(int id, UpdateProductDTO dto);
    Task DeleteAsync(int id);
    Task<bool> ReduceStockAsync(int productId, int quantity);

}