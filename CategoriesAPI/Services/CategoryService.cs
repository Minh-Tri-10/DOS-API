using AutoMapper; // Thêm using này
using CategoriesAPI.Repositories.Interfaces;
using CategoriesAPI.Services.Interfaces;
using CategoriesAPI.Models;
using CategoriesAPI.DTOs;
using AutoMapper.QueryableExtensions;

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
        public IQueryable<CategoryDTO> GetAllQueryable()
        {
            return _repository.GetAllQueryable().ProjectTo<CategoryDTO>(_mapper.ConfigurationProvider);
        }

        public async Task<CategoryDTO?> GetByIdAsync(int id)
        {
            var category = await _repository.GetByIdAsync(id);
            return _mapper.Map<CategoryDTO>(category);
        }
        public async Task<IEnumerable<CategoryDTO>> GetByIdsAsync(IEnumerable<int> ids)
        {
            var categories = await _repository.GetByIdsAsync(ids);
            return _mapper.Map<IEnumerable<CategoryDTO>>(categories);
        }

        public async Task<CategoryDTO> AddAsync(CreateCategoryDTO dto)
        {
            if (await IsNameUniqueAsync(dto.CategoryName) == false)
            {
                throw new InvalidOperationException("Tên category đã tồn tại.");
            }
            var category = _mapper.Map<Category>(dto);
            await _repository.AddAsync(category);
            return _mapper.Map<CategoryDTO>(category);
        }

        public async Task UpdateAsync(int id, UpdateCategoryDTO dto)
        {
            var category = await _repository.GetByIdAsync(id);
            if (category == null) throw new KeyNotFoundException("Category không tồn tại.");

            if (await IsNameUniqueAsync(dto.CategoryName, id) == false)
            {
                throw new InvalidOperationException("Tên category đã tồn tại.");
            }

            _mapper.Map(dto, category);
            await _repository.UpdateAsync(category);
        }

        public async Task<bool> IsNameUniqueAsync(string name, int? excludeId = null)
        {
            return !(await _repository.NameExistsAsync(name, excludeId));
        }

        public async Task DeleteAsync(int id)
        {
            await _repository.DeleteAsync(id);
        }
    }
}