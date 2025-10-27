// CategoryService.cs
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using MVCApplication.Services.Interfaces;
using MVCApplication.Models;
using MVCApplication.DTOs;

namespace MVCApplication.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public CategoryService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7021/"); // API base URL with port 7021
            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // Handle case insensitivity if needed
            };
        }

        public async Task<IEnumerable<CategoryDTO>> GetAllAsync()
        {
            var response = await _httpClient.GetAsync("api/ManageCategory");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<CategoryDTO>>(content, _jsonOptions) ?? Enumerable.Empty<CategoryDTO>();
        }

        public async Task<CategoryDTO?> GetByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/ManageCategory/{id}");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<CategoryDTO>(content, _jsonOptions);
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