using OrderAPI.DTOs;
using OrderAPI.Services.Interfaces;

namespace OrderAPI.Services
{
    public class CategoryClient : ICategoryClient
    {
        private readonly HttpClient _http;

        public CategoryClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<CategoryDto>> GetCategoriesByIdsAsync(IEnumerable<int> ids)
        {
            // giả sử CategoryAPI có endpoint: POST /api/categories/by-ids
            var response = await _http.PostAsJsonAsync("api/manageCategory/by-ids", ids);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<CategoryDto>>() ?? new();
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
        {
            return await _http.GetFromJsonAsync<CategoryDto>($"api/manageCategory/{id}");
        }
    }
}