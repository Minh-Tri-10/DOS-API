using CategoriesAPI.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using CategoriesAPI.Models;

namespace CategoriesAPI.Repositories;
public class ProductRepository : IProductRepository
{
    private readonly CatalogDbContext _context;
    public ProductRepository(CatalogDbContext context) => _context = context;

    // L?y t?t c?, Include Category
    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _context.Products
            .Include(p => p.Category) // load thêm Category
            .ToListAsync();
    }

    // L?y theo Id, Include Category
    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products
            .Include(p => p.Category) // load thêm Category
            .FirstOrDefaultAsync(p => p.ProductId == id);
    }

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
        if (product == null) return; // Ho?c throw n?u c?n

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

