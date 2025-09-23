namespace AccountAPI.DTOs
{
    public class RegisterDTO
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }        // ✅ nếu muốn cho nhập khi đăng ký
    }
}
