using OrderAPI.DTOs;

namespace OrderAPI.Services.Interfaces
{
    public interface ICategoryClient
    {
        Task<List<CategoryDto>> GetCategoriesByIdsAsync(IEnumerable<int> ids);
        Task<CategoryDto?> GetCategoryByIdAsync(int id);
    }
}
