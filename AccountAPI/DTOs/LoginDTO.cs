namespace AccountAPI.DTOs
{
    // Dữ liệu cần thiết để yêu cầu đăng nhập.
    public class LoginDTO
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
