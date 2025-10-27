using System;
using System.Net.Http;
using System.Net.Http.Json;
using MVCApplication.DTOs;
using MVCApplication.Services.Interfaces;

namespace MVCApplication.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly HttpClient _httpClient;
        private const string PaymentsEndpoint = "api/payments";

        public PaymentService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<PaymentResponseDTO>> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<PaymentResponseDTO>>(PaymentsEndpoint)
                   ?? Array.Empty<PaymentResponseDTO>();
        }

        public async Task<PaymentResponseDTO> GetByIdAsync(int id)
        {
            var payment = await _httpClient.GetFromJsonAsync<PaymentResponseDTO>($"{PaymentsEndpoint}/{id}");
            if (payment == null)
            {
                throw new InvalidOperationException("Payment not found.");
            }

            return payment;
        }

        public async Task CreateAsync(PaymentRequestDTO dto)
        {
            var response = await _httpClient.PostAsJsonAsync(PaymentsEndpoint, dto);
            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateAsync(int id, PaymentRequestDTO dto)
        {
            var response = await _httpClient.PutAsJsonAsync($"{PaymentsEndpoint}/{id}", dto);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{PaymentsEndpoint}/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}
