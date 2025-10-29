using MVCApplication.DTOs;

namespace MVCApplication.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDTO>> GetAllAsync();
        Task<CategoryDTO> GetByIdAsync(int id);
        Task<CategoryDTO> AddAsync(CreateCategoryDTO dto); // Thay đổi để return DTO
        Task UpdateAsync(int id, UpdateCategoryDTO dto);
        Task DeleteAsync(int id);

        // Method mới cho OData
        Task<(IEnumerable<CategoryDTO> Items, int TotalCount)> GetODataAsync(int page, int pageSize, string search, string orderBy);
    }
}
