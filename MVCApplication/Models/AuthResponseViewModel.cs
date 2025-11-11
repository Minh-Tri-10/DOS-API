namespace MVCApplication.Models
{
    // Map 1-1 với AuthResponseDTO từ AccountAPI (token + thông tin user).
    public class AuthResponseViewModel
    {
        public required string AccessToken { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
        public required UserViewModel User { get; set; }
    }
}
