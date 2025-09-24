using AutoMapper; // Install AutoMapper.Extensions.Microsoft.DependencyInjection
using CategoriesAPI.Services.Interfaces;
using CategoriesAPI.DTOs;
using CategoriesAPI.Models;
using CategoriesAPI.Repositories.Interfaces;
namespace ProductAPI.Services;
public class ProductService : IProductService
{
    private readonly IProductRepository _repo;
    private readonly IMapper _mapper;
    public ProductService(IProductRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ProductDTO>> GetAllAsync()
    {
        var products = await _repo.GetAllAsync();
        return _mapper.Map<IEnumerable<ProductDTO>>(products);
    }

    public async Task<ProductDTO?> GetByIdAsync(int id)
    {
        var product = await _repo.GetByIdAsync(id);
        if (product == null) throw new KeyNotFoundException($"Product {id} not found");
        return _mapper.Map<ProductDTO>(product);
    }

    public async Task<ProductDTO> CreateAsync(CreateProductDTO DTO)
    {
        try
        {
            var product = _mapper.Map<Product>(DTO);
            product.CreatedAt = DateTime.UtcNow;
            await _repo.AddAsync(product);
            return _mapper.Map<ProductDTO>(product);
        }
        catch (Exception ex)
        {
            throw new Exception("Error creating product", ex);
        }
    }

    public async Task UpdateAsync(int id, UpdateProductDTO DTO)
    {
        var product = await _repo.GetByIdAsync(id);
        if (product == null) throw new KeyNotFoundException($"Product {id} not found");

        try
        {
            _mapper.Map(DTO, product);
            product.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(product);
        }
        catch (Exception ex)
        {
            throw new Exception("Error updating product", ex);
        }
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _repo.GetByIdAsync(id);
        if (product == null) throw new KeyNotFoundException($"Product {id} not found");

        try
        {
            await _repo.DeleteAsync(id);
        }
        catch (Exception ex)
        {
            throw new Exception("Error deleting product", ex);
        }
    }
}