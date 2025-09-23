namespace MVCApplication.Services
{
    using System.Net.Http.Json;
    using MVCApplication.Models;
    using MVCApplication.Services.Interfaces;

    public class AccountService : IAccountService
    {
        private readonly HttpClient _http;
        public AccountService(HttpClient http) => _http = http;

        public async Task<UserViewModel?> LoginAsync(LoginViewModel dto)
        {
            var res = await _http.PostAsJsonAsync("api/accounts/login", dto);
            if (!res.IsSuccessStatusCode) return null;
            return await res.Content.ReadFromJsonAsync<UserViewModel>();
        }

        public async Task<UserViewModel?> RegisterAsync(RegisterViewModel dto)
        {
            var res = await _http.PostAsJsonAsync("api/accounts/register", dto);
            if (!res.IsSuccessStatusCode) return null;
            return await res.Content.ReadFromJsonAsync<UserViewModel>();
        }

        public Task<UserViewModel?> GetByIdAsync(int id) =>
            _http.GetFromJsonAsync<UserViewModel>($"api/accounts/{id}");
        public async Task<IEnumerable<UserViewModel>> GetAllAsync()
        {
            return await _http.GetFromJsonAsync<IEnumerable<UserViewModel>>("api/accounts");
        }
        public async Task<bool> SetBanAsync(int id, bool isBanned)
        {
            var res = await _http.PatchAsJsonAsync($"api/accounts/{id}/ban", new { IsBanned = isBanned });
            return res.IsSuccessStatusCode;
        }
        public async Task<bool> ForgotPasswordAsync(string email)
        {
            var res = await _http.PostAsJsonAsync("api/accounts/forgot-password", new { Email = email });
            return res.IsSuccessStatusCode; // 204 NoContent => true
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            var res = await _http.PostAsJsonAsync("api/accounts/reset-password", new { Token = token, NewPassword = newPassword });
            return res.IsSuccessStatusCode; // 204 NoContent => true
        }
    }

}
