using CategoriesAPI.DTOs;
using CategoriesAPI.Models;
using CategoriesAPI.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;

namespace CategoriesAPI.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly CatalogDbContext _context; // Gi? d?nh DbContext c?a b?n

        public CategoryRepository(CatalogDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _context.Categories.ToListAsync();
        }

        public IQueryable<Category> GetAllQueryable()
        {
            return _context.Categories.AsQueryable();
            //return await _context.Categories.ToListAsync();
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == id);
        }
        public async Task<IEnumerable<Category>> GetByIdsAsync(IEnumerable<int> ids)
        {
            return await _context.Categories
                                 .Where(c => ids.Contains(c.CategoryId))
                                 .ToListAsync();
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
                _context.Categories.Remove(category);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    // N?u l?i do ràng bu?c FK
                    if (ex.InnerException is SqlException sqlEx && sqlEx.Number == 547)
                    {
                        throw new InvalidOperationException("Không th? xóa category vì có d? li?u khác dang tham chi?u.", ex);
                    }

                    // N?u l?i khác thì ném ti?p
                    throw;
                }
            }
        }


        //public async Task<bool> HasProductsAsync(int categoryId)
        //{
        //    return await _context.Products.AnyAsync(p => p.CategoryId == categoryId);
        //}

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
