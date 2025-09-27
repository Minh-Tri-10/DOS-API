using System.Net.Http;
using System.Net.Http.Json;
using MVCApplication.DTOs;
using MVCApplication.Services.Interfaces;

namespace MVCApplication.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly HttpClient _httpClient;
        private readonly string ApiUrl = "https://localhost:7011/api/payments";

        public PaymentService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // GET: /api/payments
        public async Task<IEnumerable<PaymentResponseDTO>> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<PaymentResponseDTO>>($"{ApiUrl}");
        }

        // GET: /api/payments/{id}
        public async Task<PaymentResponseDTO> GetByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<PaymentResponseDTO>($"{ApiUrl}/{id}");
        }

        // POST: /api/payments
        public async Task CreateAsync(PaymentRequestDTO dto)
        {
            var response = await _httpClient.PostAsJsonAsync($"{ApiUrl}", dto);
            response.EnsureSuccessStatusCode();
        }

        // PUT: /api/payments/{id}
        public async Task UpdateAsync(int id, PaymentRequestDTO dto)
        {
            var response = await _httpClient.PutAsJsonAsync($"{ApiUrl}/{id}", dto);
            response.EnsureSuccessStatusCode();
        }

        // DELETE: /api/payments/{id}
        public async Task DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{ApiUrl}/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}
