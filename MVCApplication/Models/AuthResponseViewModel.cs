namespace MVCApplication.Models
{
    public class AuthResponseViewModel
    {
        public required string AccessToken { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
        public required UserViewModel User { get; set; }
    }
}
