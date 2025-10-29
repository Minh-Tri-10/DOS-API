using CategoriesAPI.DTOs;
using CategoriesAPI.Models;

namespace CategoriesAPI.Repositories.Interfaces
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllAsync();
        IQueryable<Category> GetAllQueryable();
        Task<Category?> GetByIdAsync(int id);
        Task AddAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(int id);
        //Task<bool> HasProductsAsync(int categoryId); // Kiểm tra mối quan hệ với Product
        Task<bool> NameExistsAsync(string name, int? excludeId = null);  // Mới
        Task<IEnumerable<Category>> GetByIdsAsync(IEnumerable<int> ids);

    }
}
