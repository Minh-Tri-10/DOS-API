using AutoMapper; // Thêm using này
using CategoriesAPI.Repositories.Interfaces;
using CategoriesAPI.Services.Interfaces;
using CategoriesAPI.Models;
using CategoriesAPI.DTOs;

namespace CategoriesAPI.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repository;
        private readonly IMapper _mapper;

        public CategoryService(ICategoryRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CategoryDTO>> GetAllAsync()
        {
            var categories = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<CategoryDTO>>(categories);
        }

        public async Task<CategoryDTO?> GetByIdAsync(int id)
        {
            var category = await _repository.GetByIdAsync(id);
            return _mapper.Map<CategoryDTO>(category);
        }

        public async Task<CategoryDTO> AddAsync(CreateCategoryDTO dto) // Thay đổi return type thành CategoryDTO hoặc Category
        {
            var category = _mapper.Map<Category>(dto);
            await _repository.AddAsync(category);
            return _mapper.Map<CategoryDTO>(category); // Return DTO với Id mới
        }

        public async Task UpdateAsync(int id, UpdateCategoryDTO dto)
        {
            var category = await _repository.GetByIdAsync(id);
            if (category != null)
            {
                _mapper.Map(dto, category); // Update fields
                await _repository.UpdateAsync(category);
            }
        }

        public async Task DeleteAsync(int id)
        {
            await _repository.DeleteAsync(id);
        }
    }
}