using MVCApplication.DTOs;

namespace MVCApplication.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDTO>> GetAllAsync();
        Task<ProductDTO?> GetByIdAsync(int id);
        Task<ProductDTO> CreateAsync(CreateProductDTO dto, IFormFile? imageFile);
        Task UpdateAsync(int id, UpdateProductDTO dto);
        Task DeleteAsync(int id);

        Task<(IEnumerable<ProductDTO> Items, int TotalCount)> GetODataAsync(int page, int pageSize, string search, string orderBy, int? categoryId = null);
    }
}
