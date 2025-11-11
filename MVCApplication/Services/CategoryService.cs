using Microsoft.AspNetCore.Http;
using MVCApplication.DTOs;
using MVCApplication.Models;
using MVCApplication.Services.Interfaces;
using System;
using System.Net.Http.Json;
using System.Text.Json;

namespace MVCApplication.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CategoryService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClientFactory.CreateClient("CategoriesAPI");
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            _httpContextAccessor = httpContextAccessor;
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

      

        // Method mới cho OData: Build query và gọi API
        public async Task<(IEnumerable<CategoryDTO> Items, int TotalCount)> GetODataAsync(int page, int pageSize, string search, string orderBy)
        {
            // Lấy token từ HttpContext.User.Claims (key "access_token" như trong controller)
            var token = _httpContextAccessor.HttpContext?.User.FindFirst("access_token")?.Value;
            if (string.IsNullOrEmpty(token))
            {
                throw new InvalidOperationException("Không tìm thấy access token. Vui lòng đăng nhập lại.");
            }

            // Set header Authorization
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Build query
            var skip = (page - 1) * pageSize;
            var filter = string.IsNullOrWhiteSpace(search) ? "" : $"contains(CategoryName,'{Uri.EscapeDataString(search)}')";  // Thêm escape để tránh lỗi special char
            var queryParts = new List<string>();
            if (!string.IsNullOrWhiteSpace(filter))
            {
                queryParts.Add($"$filter={filter}");
            }
            queryParts.Add($"$orderby={orderBy.Replace(" ", "%20")}");  // Encode space trong orderBy để tránh lỗi URL
            queryParts.Add($"$top={pageSize}");
            queryParts.Add($"$skip={skip}");
            queryParts.Add("$count=true");
            var query = $"odata/Categories?{string.Join("&", queryParts)}";

            // Log query để debug
            Console.WriteLine("OData Query: " + query);  // Hoặc dùng ILogger nếu có

            var response = await _httpClient.GetAsync(query);
            var content = await response.Content.ReadAsStringAsync();

            // Log content trước deserialize
            Console.WriteLine("OData Response Content: " + content);
            Console.WriteLine("Response Status: " + response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Lỗi gọi API: {response.StatusCode} - {content}");
            }

            // Deserialize với options linh hoạt hơn
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,  // Cho phép dư dấu phẩy
                ReadCommentHandling = JsonCommentHandling.Skip  // Bỏ qua comment nếu có
            };

            try
            {
                var odataResponse = JsonSerializer.Deserialize<ODataCateResponse>(content, _jsonOptions);
                return (odataResponse?.Value ?? Enumerable.Empty<CategoryDTO>(), odataResponse?.Count ?? 0);
            }
            catch
            {
                // Fallback: Deserialize trực tiếp thành mảng
                var items = JsonSerializer.Deserialize<IEnumerable<CategoryDTO>>(content, _jsonOptions) ?? Enumerable.Empty<CategoryDTO>();
                return (items, items.Count());
            }
        }
    }
}
