using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MVCApplication.Infrastructure
{
    // DelegatingHandler chạy trước mỗi HttpClient call để tự động gắn JWT lấy từ cookie đăng nhập.
    public class AccessTokenHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AccessTokenHandler> _logger;

        public AccessTokenHandler(IHttpContextAccessor httpContextAccessor, ILogger<AccessTokenHandler> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            // JWT được lưu trong claim "access_token" lúc người dùng đăng nhập.
            var token = httpContext?.User?.FindFirst("access_token")?.Value;

            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("Missing bearer token for outbound request {Method} {Uri}", request.Method, request.RequestUri);
            }
            else if (request.Headers.Authorization == null)
            {
                // Đính token vào header Authorization nếu caller chưa set.
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                _logger.LogDebug("Attached bearer token to outbound request {Method} {Uri}", request.Method, request.RequestUri);
            }

            var response = await base.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogWarning(
                    "Downstream call {Method} {Uri} failed with status {StatusCode}. Response: {Body}",
                    request.Method,
                    request.RequestUri,
                    (int)response.StatusCode,
                    body);
            }

            return response;
        }
    }
}
