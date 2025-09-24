using System.ComponentModel.DataAnnotations;

namespace MVCApplication.Models
{
    public class UpdateProfileViewModel
    {
        // ========== Các field không update (chỉ để hiển thị) ==========
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? Role { get; set; }
        public bool IsBanned { get; set; }

        // ========== Các field cho phép update ==========
        [Required, StringLength(100)]
        public string? FullName { get; set; }

        [EmailAddress, StringLength(100)]
        public string? Email { get; set; }

        [Phone, StringLength(20)]
        public string? Phone { get; set; }

        [Url, StringLength(255)]
        public string? AvatarUrl { get; set; }
    }
}
