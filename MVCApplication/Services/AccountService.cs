namespace MVCApplication.Services
{
    using Microsoft.Extensions.Logging;
    using MVCApplication.Models;
    using MVCApplication.Services.Interfaces;
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Net;
    using System.Net.Http.Json;
    using System.Text.Json;

    // Bao dong wrapper HttpClient de giao tiep voi Gateway/AccountAPI tu MVC.
    public class AccountService : IAccountService
    {
        private readonly HttpClient _http;
        private readonly ILogger<AccountService> _logger;
        public AccountService(HttpClient http, ILogger<AccountService> logger)
        {
            _http = http;
            _logger = logger;
        }

        // Gửi request đăng nhập và parse AuthResponse; trả null nếu API từ chối.
        public async Task<AuthResponseViewModel?> LoginAsync(LoginViewModel dto)
        {
            var res = await _http.PostAsJsonAsync("api/accounts/login", dto);
            if (!res.IsSuccessStatusCode)
            {
                var payload = await res.Content.ReadAsStringAsync();
                _logger.LogWarning("POST api/accounts/login failed with {StatusCode}. Body: {Body}", (int)res.StatusCode, payload);

                var message = ExtractErrorMessage(payload);
                if (string.IsNullOrWhiteSpace(message))
                {
                    message = res.StatusCode == HttpStatusCode.Forbidden
                        ? "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ hỗ trợ."
                        : "Sai tài khoản hoặc mật khẩu.";
                }
                throw new InvalidOperationException(message);
            }
            return await res.Content.ReadFromJsonAsync<AuthResponseViewModel>();
        }

        // Đăng ký tài khoản qua gateway; ném InvalidOperationException khi API trả lỗi nghiệp vụ.
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

        // Lấy thông tin người dùng theo id phục vụ trang profile.
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
        // Admin UI dùng để liệt kê toàn bộ user.
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
        // Kích hoạt/huỷ trạng thái ban trên AccountAPI.
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
        // Gửi yêu cầu quên mật khẩu.
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

        // Hoàn tất đặt lại mật khẩu bằng token đã nhận qua email.
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
        // Upload form-data (text + ảnh) tới AccountAPI để cập nhật hồ sơ người dùng.
        public async Task<UserViewModel?> UpdateProfileAsync(int userId, UpdateProfileViewModel dto, IFormFile? avatarFile)
        {
            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(userId.ToString()), "UserId");
            if (!string.IsNullOrWhiteSpace(dto.FullName))
                content.Add(new StringContent(dto.FullName), "FullName");
            if (!string.IsNullOrWhiteSpace(dto.Email))
                content.Add(new StringContent(dto.Email), "Email");
            if (!string.IsNullOrWhiteSpace(dto.Phone))
                content.Add(new StringContent(dto.Phone), "Phone");
            if (!string.IsNullOrWhiteSpace(dto.AvatarUrl))
                content.Add(new StringContent(dto.AvatarUrl), "AvatarUrl");

            if (avatarFile != null && avatarFile.Length > 0)
            {
                var stream = avatarFile.OpenReadStream();
                content.Add(new StreamContent(stream), "avatarFile", avatarFile.FileName);
            }

            var response = await _http.PutAsync($"api/accounts/{userId}/profile", content);

            if (response.IsSuccessStatusCode)
            {
                var user = await response.Content.ReadFromJsonAsync<UserViewModel>();
                return user;
            }

            // ✅ Nếu lỗi 400 => lấy thông điệp lỗi cụ thể từ API
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var errorJson = await response.Content.ReadAsStringAsync();

                try
                {
                    var err = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(errorJson);

                    // ASP.NET Core ModelState trả về {"errors":{"Phone":["Số điện thoại đã được sử dụng."]}}
                    if (err.TryGetProperty("errors", out var errorsProp) && errorsProp.ValueKind == JsonValueKind.Object)
                    {
                        var errorsDict = new Dictionary<string, string[]>();
                        foreach (var kv in errorsProp.EnumerateObject())
                        {
                            var messages = kv.Value.EnumerateArray().Select(v => v.GetString() ?? "").ToArray();
                            errorsDict[kv.Name] = messages;
                        }

                        // Ném lỗi ValidationException có JSON để controller MVC đọc lại
                        throw new ValidationException(System.Text.Json.JsonSerializer.Serialize(errorsDict));
                    }

                    // Nếu API trả message trực tiếp
                    if (err.TryGetProperty("message", out var msg))
                        throw new ValidationException(msg.GetString() ?? "Dữ liệu không hợp lệ.");
                }
                catch (ValidationException)
                {
                    throw; // Giữ nguyên
                }
                catch
                {
                    throw new ValidationException("Dữ liệu không hợp lệ.");
                }
            }


            // ✅ Nếu lỗi 404
            if (response.StatusCode == HttpStatusCode.NotFound)
                throw new InvalidOperationException("Không tìm thấy người dùng.");

            // ✅ Nếu lỗi khác
            throw new HttpRequestException($"API Error: {response.StatusCode}");
        }


        // Cố gắng rút ra thông báo cụ thể từ response JSON (ModelState/ProblemDetails).
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
