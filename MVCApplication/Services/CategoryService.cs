using System;
using System.Net.Http.Json;
using System.Text.Json;
using MVCApplication.DTOs;
using MVCApplication.Models;
using MVCApplication.Services.Interfaces;

namespace MVCApplication.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public CategoryService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ProductAPI");
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<IEnumerable<CategoryDTO>> GetAllAsync()
        {
            var response = await _httpClient.GetAsync("api/ManageCategory");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<CategoryDTO>>(content, _jsonOptions) ?? Enumerable.Empty<CategoryDTO>();
        }

        public async Task<CategoryDTO> GetByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/ManageCategory/{id}");
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(error) ? "Unable to load category." : error);
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<CategoryDTO>(content, _jsonOptions)
                   ?? throw new InvalidOperationException("Category payload is empty.");
        }

        public async Task<CategoryDTO> AddAsync(CreateCategoryDTO dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/ManageCategory", dto);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException(errorContent);
            }
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<CategoryDTO>(content, _jsonOptions) ?? throw new InvalidOperationException("Failed to deserialize new category");
        }

        public async Task UpdateAsync(int id, UpdateCategoryDTO dto)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/ManageCategory/{id}", dto);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException(errorContent);
            }
        }

        public async Task DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/ManageCategory/{id}");
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException(errorContent);
            }
        }
    }
}
