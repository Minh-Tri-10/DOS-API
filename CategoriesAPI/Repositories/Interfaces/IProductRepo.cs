using CategoriesAPI.DTOs;
using CategoriesAPI.Models;

namespace CategoriesAPI.Repositories.Interfaces;
public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync();
    IQueryable<Product> GetAllQueryable();
    Task<Product?> GetByIdAsync(int id);
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(int id);
}