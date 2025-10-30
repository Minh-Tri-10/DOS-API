using MVCApplication.Services.Interfaces;
using MVCApplication.DTOs;
using System.Text.Json;
using System.Text;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MVCApplication.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUri = "api/Feedbacks"; // Endpoint cơ sở của API

        // HttpClient đã được cấu hình BaseAddress (địa chỉ của API) trong Program.cs
        public FeedbackService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // --- GET ALL ---
        public async Task<List<FeedbackResponseDTO>?> GetAllFeedbacksAsync()
        {
            var response = await _httpClient.GetAsync(BaseUri);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                // Phân tích JSON thành List<FeedbackResponseDTO>
                return JsonSerializer.Deserialize<List<FeedbackResponseDTO>>(
                    content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            return null;
        }

        // --- GET BY ID ---
        public async Task<FeedbackResponseDTO?> GetFeedbackByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"{BaseUri}/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                // Phân tích JSON thành FeedbackResponseDTO
                return JsonSerializer.Deserialize<FeedbackResponseDTO>(
                    content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            return null;
        }

        // --- POST (CREATE) ---
        public async Task<bool> CreateFeedbackAsync(FeedbackRequestDTO createDto)
        {
            // Chuyển DTO thành chuỗi JSON
            var jsonContent = JsonSerializer.Serialize(createDto);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(BaseUri, content);
            // Trả về true nếu nhận được 2xx status code
            return response.IsSuccessStatusCode;
        }

        // --- PUT (UPDATE) ---
        public async Task<bool> UpdateFeedbackAsync(int id, FeedbackUpdateDTO updateDto)
        {
            // Chuyển DTO thành chuỗi JSON
            var jsonContent = JsonSerializer.Serialize(updateDto);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"{BaseUri}/{id}", content);
            // Trả về true nếu nhận được 2xx status code
            return response.IsSuccessStatusCode;
        }

        // --- DELETE ---
        public async Task<bool> DeleteFeedbackAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{BaseUri}/{id}");
            // Trả về true nếu nhận được 2xx status code
            return response.IsSuccessStatusCode;
        }
        // 🚀 PHƯƠNG THỨC MỚI: LẤY THEO ORDER ID (int)
        public async Task<IEnumerable<FeedbackResponseDTO>> GetFeedbacksByOrderIdAsync(int orderId)
        {
            // Gọi đến endpoint đã cấu hình trong Controller: api/Feedbacks/byorder/{orderId}
            var response = await _httpClient.GetAsync($"{BaseUri}/byorder/{orderId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                // Phân tích JSON thành IEnumerable<FeedbackResponseDTO>
                return JsonSerializer.Deserialize<IEnumerable<FeedbackResponseDTO>>(
                    content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? Enumerable.Empty<FeedbackResponseDTO>(); // Trả về danh sách rỗng nếu Deserialize trả về null
            }

            // Trả về danh sách rỗng nếu request không thành công (ví dụ: 404 Not Found)
            return Enumerable.Empty<FeedbackResponseDTO>();
        }
    }
}