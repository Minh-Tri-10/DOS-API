using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AccountAPI.Services.Email
{
    public class EmailJsSender : IEmailSender
    {
        private static readonly HttpClient Http = new HttpClient();
        private readonly EmailOptions _opt;

        public EmailJsSender(EmailOptions opt)
        {
            _opt = opt;
        }

        public async Task SendAsync(string to, string subject, string htmlBody)
        {
            const string endpoint = "https://api.emailjs.com/api/v1.0/email/send";

            var payload = new
            {
                service_id = _opt.ServiceId,
                template_id = _opt.TemplateId,
                user_id = _opt.PublicKey,
                accessToken = _opt.AccessToken,
                template_params = new
                {
                    to_email = to,
                    subject,
                    html = htmlBody,
                    from_name = _opt.FromName
                }
            };

            var json = JsonSerializer.Serialize(payload);
            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            if (!string.IsNullOrWhiteSpace(_opt.AccessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _opt.AccessToken);
            }

            if (!string.IsNullOrWhiteSpace(_opt.Origin))
            {
                request.Headers.TryAddWithoutValidation("Origin", _opt.Origin);
            }

            using var response = await Http.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"EmailJS returned {(int)response.StatusCode} {response.StatusCode}: {body}");
            }
        }
    }
}

