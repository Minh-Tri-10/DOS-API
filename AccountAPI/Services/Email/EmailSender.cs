using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace AccountAPI.Services.Email
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailOptions _opt;
        public EmailSender(EmailOptions opt) => _opt = opt;

        public async Task SendAsync(string to, string subject, string htmlBody)
        {
            using var msg = new MailMessage
            {
                From = new MailAddress(_opt.From, _opt.FromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            msg.To.Add(to);

            using var client = new SmtpClient(_opt.SmtpHost, _opt.SmtpPort)
            {
                Credentials = string.IsNullOrEmpty(_opt.Username)
                    ? CredentialCache.DefaultNetworkCredentials
                    : new NetworkCredential(_opt.Username, _opt.Password),
                EnableSsl = _opt.UseSsl
            };

            await client.SendMailAsync(msg);
        }
    }
}
