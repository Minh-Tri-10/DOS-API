using Microsoft.EntityFrameworkCore;
using ProductAPI.Models;
using ProductAPI.Repositories.Interfaces;
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
        if (product != null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
    }
}