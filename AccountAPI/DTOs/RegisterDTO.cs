using System.ComponentModel.DataAnnotations;

namespace AccountAPI.DTOs
{
    // DTO gửi từ client khi đăng ký tài khoản tại AccountAPI.
    public class RegisterDTO
    {
        [Required]
        [StringLength(50, MinimumLength = 4)]
        public string Username { get; set; } = null!;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = null!;

        [StringLength(100)]
        public string? FullName { get; set; }

        [EmailAddress]
        [StringLength(150)]
        public string? Email { get; set; }

        [Phone]
        [StringLength(20)]
        public string? Phone { get; set; }
    }
}
