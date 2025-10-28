using AutoMapper; // Install AutoMapper.Extensions.Microsoft.DependencyInjection
using CategoriesAPI.Services.Interfaces;
using CategoriesAPI.DTOs;
using CategoriesAPI.Models;
using CategoriesAPI.Repositories.Interfaces;

namespace CategoriesAPI.Services;
public class ProductService : IProductService
{
    private readonly IProductRepository _repo;
    private readonly IMapper _mapper;
    private readonly ICloudinaryService _cloudinaryService;
    public ProductService(IProductRepository repo, IMapper mapper, ICloudinaryService cloudinaryService)
    {
        _repo = repo;
        _mapper = mapper;
        _cloudinaryService = cloudinaryService;
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

    public async Task<ProductDTO> CreateAsync(CreateProductDTO dto)
    {
        try
        {
            Console.WriteLine($"Received ImageFile: {(dto.ImageFile != null ? "Có file" : "Null")}");
            Console.WriteLine($"Received ImageUrl: {dto.ImageUrl ?? "Null"}");

            if (dto.ImageFile != null)
            {
                dto.ImageUrl = await _cloudinaryService.UploadImageAsync(dto.ImageFile);
                Console.WriteLine($"Uploaded ImageUrl: {dto.ImageUrl}");
            }
            var product = _mapper.Map<Product>(dto);
            product.CreatedAt = DateTime.UtcNow;
            await _repo.AddAsync(product);
            return _mapper.Map<ProductDTO>(product);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating product: {ex.Message}");
            throw new Exception("Error creating product", ex);
        }
    }

    public async Task UpdateAsync(int id, UpdateProductDTO dto)
    {
        var product = await _repo.GetByIdAsync(id);
        if (product == null) throw new KeyNotFoundException($"Product {id} not found");
        try
        {
            Console.WriteLine($"Received ImageFile: {(dto.ImageFile != null ? "Có file" : "Null")}");
            Console.WriteLine($"Received ImageUrl: {dto.ImageUrl ?? "Null"}");

            if (dto.ImageFile != null)
            {
                dto.ImageUrl = await _cloudinaryService.UploadImageAsync(dto.ImageFile);
                Console.WriteLine($"Uploaded ImageUrl: {dto.ImageUrl}");
            }
            _mapper.Map(dto, product);
            product.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(product);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating product: {ex.Message}");
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

