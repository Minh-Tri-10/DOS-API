namespace AccountAPI.DTOs
{
    public class AuthResponseDTO
    {
        public required string AccessToken { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
        public required UserDTO User { get; set; }
    }
}
