using CategoriesAPI.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using CategoriesAPI.Models;
namespace ProductAPI.Repositories;
public class ProductRepository : IProductRepository
{
    private readonly DrinkOrderDbContext _context;
    public ProductRepository(DrinkOrderDbContext context) => _context = context;

    public async Task<IEnumerable<Product>> GetAllAsync() => await _context.Products.ToListAsync();

    public async Task<Product?> GetByIdAsync(int id) => await _context.Products.FindAsync(id);

    public async Task AddAsync(Product product)
    {
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Product product)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var product = await GetByIdAsync(id);
        if (product == null) return; // Hoặc throw nếu cần

        try
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number == 547)
        {
            throw new InvalidOperationException("Cannot delete product {id} because it is referenced by other entities.");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error deleting product {id}");
        }
    }
}