namespace MVCApplication.Services
{
    using System;
    using System.Net;
    using System.Net.Http.Json;
    using System.Text.Json;
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
            if (res.IsSuccessStatusCode)
            {
                return await res.Content.ReadFromJsonAsync<UserViewModel>();
            }

            var payload = await res.Content.ReadAsStringAsync();

            if (res.StatusCode == HttpStatusCode.Conflict || res.StatusCode == HttpStatusCode.BadRequest)
            {
                var message = ExtractErrorMessage(payload) ?? "Đăng ký thất bại.";
                throw new InvalidOperationException(message);
            }

            _logger.LogWarning("POST api/accounts/register failed with {StatusCode}. Body: {Body}", (int)res.StatusCode, payload);
            res.EnsureSuccessStatusCode();
            return null;
        }

        public async Task<UserViewModel?> GetByIdAsync(int id)
        {
            var response = await _http.GetAsync($"api/accounts/{id}");
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("GET api/accounts/{Id} returned 404 – returning null to caller", id);
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                var payload = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("GET api/accounts/{Id} failed with {StatusCode}. Body: {Body}", id, (int)response.StatusCode, payload);
                response.EnsureSuccessStatusCode();
            }

            return await response.Content.ReadFromJsonAsync<UserViewModel>();
        }
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

        private static string? ExtractErrorMessage(string? payload)
        {
            if (string.IsNullOrWhiteSpace(payload))
                return null;

            try
            {
                using var doc = JsonDocument.Parse(payload);
                var root = doc.RootElement;

                if (root.ValueKind == JsonValueKind.Object)
                {
                    if (root.TryGetProperty("message", out var messageProp))
                        return messageProp.GetString();

                    if (root.TryGetProperty("title", out var titleProp))
                        return titleProp.GetString();

                    if (root.TryGetProperty("errors", out var errorsProp) && errorsProp.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var error in errorsProp.EnumerateObject())
                        {
                            if (error.Value.ValueKind == JsonValueKind.Array && error.Value.GetArrayLength() > 0)
                                return error.Value[0].GetString();
                        }
                    }
                }
            }
            catch
            {
                return payload;
            }

            return payload;
        }

    }

}
