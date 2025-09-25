using OrderAPI.Services.Interfaces;

namespace OrderAPI.Services
{
    public class UserClient : IUserClient
    {
        private readonly HttpClient _http;

        public UserClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<string?> GetFullNameByIdAsync(int userId)
        {
            // giả sử UserService có endpoint: GET /api/accounts/{id}
            var user = await _http.GetFromJsonAsync<UserDto>($"api/accounts/{userId}");
            return user?.FullName;
        }

        private class UserDto
        {
            public int UserId { get; set; }
            public string FullName { get; set; } = "";
        }
    }
}
