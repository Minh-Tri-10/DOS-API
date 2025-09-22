using System.Net.Http.Json;
using WebApp.Models;
using WebApp.Services.Interfaces;

namespace WebApp.Services
{
    public class AccountService : IAccountService
    {
        private readonly HttpClient _httpClient;
        public AccountService(HttpClient httpClient) => _httpClient = httpClient;

        public async Task<UserViewModel?> LoginAsync(LoginViewModel dto)
        {
            var res = await _httpClient.PostAsJsonAsync("api/accounts/login", dto);
            if (!res.IsSuccessStatusCode) return null;
            return await res.Content.ReadFromJsonAsync<UserViewModel>();
        }

        public async Task<UserViewModel?> RegisterAsync(RegisterViewModel dto)
        {
            var res = await _httpClient.PostAsJsonAsync("api/accounts/register", dto);
            if (!res.IsSuccessStatusCode) return null;
            return await res.Content.ReadFromJsonAsync<UserViewModel>();
        }

        public Task<UserViewModel?> GetByIdAsync(int id) =>
            _httpClient.GetFromJsonAsync<UserViewModel>($"api/accounts/{id}");
    }
}
