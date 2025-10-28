namespace MVCApplication.Services
{
    using System.Net.Http.Json;
    using Microsoft.Extensions.Logging;
    using MVCApplication.Models;
    using MVCApplication.Services.Interfaces;

    public class AccountService : IAccountService
    {
        private readonly HttpClient _http;
        private readonly ILogger<AccountService> _logger;
        public AccountService(HttpClient http, ILogger<AccountService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<AuthResponseViewModel?> LoginAsync(LoginViewModel dto)
        {
            var res = await _http.PostAsJsonAsync("api/accounts/login", dto);
            if (!res.IsSuccessStatusCode)
            {
                var payload = await res.Content.ReadAsStringAsync();
                _logger.LogWarning("POST api/accounts/login failed with {StatusCode}. Body: {Body}", (int)res.StatusCode, payload);
                return null;
            }
            return await res.Content.ReadFromJsonAsync<AuthResponseViewModel>();
        }

        public async Task<UserViewModel?> RegisterAsync(RegisterViewModel dto)
        {
            var res = await _http.PostAsJsonAsync("api/accounts/register", dto);
            if (!res.IsSuccessStatusCode)
            {
                var payload = await res.Content.ReadAsStringAsync();
                _logger.LogWarning("POST api/accounts/register failed with {StatusCode}. Body: {Body}", (int)res.StatusCode, payload);
                return null;
            }
            return await res.Content.ReadFromJsonAsync<UserViewModel>();
        }

        public Task<UserViewModel?> GetByIdAsync(int id) =>
            _http.GetFromJsonAsync<UserViewModel>($"api/accounts/{id}");
        public async Task<IEnumerable<UserViewModel>> GetAllAsync()
        {
            var response = await _http.GetAsync("api/accounts");
            if (!response.IsSuccessStatusCode)
            {
                var payload = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("GET api/accounts failed with {StatusCode}. Body: {Body}", (int)response.StatusCode, payload);
                response.EnsureSuccessStatusCode();
            }
            var users = await response.Content.ReadFromJsonAsync<IEnumerable<UserViewModel>>();
            return users ?? Enumerable.Empty<UserViewModel>();
        }
        public async Task<bool> SetBanAsync(int id, bool isBanned)
        {
            var res = await _http.PatchAsJsonAsync($"api/accounts/{id}/ban", new { IsBanned = isBanned });
            if (!res.IsSuccessStatusCode)
            {
                var payload = await res.Content.ReadAsStringAsync();
                _logger.LogWarning("PATCH api/accounts/{Id}/ban failed with {StatusCode}. Body: {Body}", id, (int)res.StatusCode, payload);
            }
            return res.IsSuccessStatusCode;
        }
        public async Task<bool> ForgotPasswordAsync(string email)
        {
            var res = await _http.PostAsJsonAsync("api/accounts/forgot-password", new { Email = email });
            if (!res.IsSuccessStatusCode)
            {
                var payload = await res.Content.ReadAsStringAsync();
                _logger.LogWarning("POST api/accounts/forgot-password failed with {StatusCode}. Body: {Body}", (int)res.StatusCode, payload);
            }
            return res.IsSuccessStatusCode; // 204 NoContent => true
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            var res = await _http.PostAsJsonAsync("api/accounts/reset-password", new { Token = token, NewPassword = newPassword });
            if (!res.IsSuccessStatusCode)
            {
                var payload = await res.Content.ReadAsStringAsync();
                _logger.LogWarning("POST api/accounts/reset-password failed with {StatusCode}. Body: {Body}", (int)res.StatusCode, payload);
            }
            return res.IsSuccessStatusCode; // 204 NoContent => true
        }
        public async Task<UserViewModel?> UpdateProfileAsync(int id, UpdateProfileViewModel dto, IFormFile? avatarFile)
        {
            using var formData = new MultipartFormDataContent();

            formData.Add(new StringContent(dto.FullName ?? ""), "FullName");
            formData.Add(new StringContent(dto.Email ?? ""), "Email");
            formData.Add(new StringContent(dto.Phone ?? ""), "Phone");

            if (avatarFile != null)
            {
                var stream = avatarFile.OpenReadStream();
                formData.Add(new StreamContent(stream), "avatarFile", avatarFile.FileName);
            }

            var res = await _http.PutAsync($"api/accounts/{id}/profile", formData);
            if (!res.IsSuccessStatusCode) return null;

            return await res.Content.ReadFromJsonAsync<UserViewModel>();
        }

    }

}
