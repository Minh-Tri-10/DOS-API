using CategoriesAPI.Models;
using CategoriesAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;

namespace CategoriesAPI.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly DrinkOrderDbContext _context; // Giả định DbContext của bạn

        public CategoryRepository(DrinkOrderDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _context.Categories.Include(c => c.Products).ToListAsync();
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Categories.Include(c => c.Products).FirstOrDefaultAsync(c => c.CategoryId == id);
        }

        public async Task AddAsync(Category category)
        {
            category.CreatedAt = DateTime.UtcNow;
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Category category)
        {
            var existing = await _context.Categories.FindAsync(category.CategoryId);
            if (existing != null)
            {
                existing.CategoryName = category.CategoryName;
                existing.Description = category.Description;
                existing.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                if (await HasProductsAsync(id))
                {
                    throw new InvalidOperationException("Không thể xóa category có product liên kết.");
                }
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> HasProductsAsync(int categoryId)
        {
            return await _context.Products.AnyAsync(p => p.CategoryId == categoryId);
        }

        public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
        {
            name = name.ToLower();

            var query = _context.Categories
                .Where(c => c.CategoryName.ToLower() == name);

            if (excludeId.HasValue)
            {
                query = query.Where(c => c.CategoryId != excludeId.Value);
            }

            return await query.AnyAsync();
        }

    }
}
