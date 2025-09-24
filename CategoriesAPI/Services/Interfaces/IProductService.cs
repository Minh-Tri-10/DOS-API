using CategoriesAPI.DTOs;
namespace CategoriesAPI.Services.Interfaces;
public interface IProductService
{
    Task<IEnumerable<ProductDTO>> GetAllAsync();
    Task<ProductDTO?> GetByIdAsync(int id);
    Task<ProductDTO> CreateAsync(CreateProductDTO dto);
    Task UpdateAsync(int id, UpdateProductDTO dto);
    Task DeleteAsync(int id);
}