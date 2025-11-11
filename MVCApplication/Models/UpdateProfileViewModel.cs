using System.ComponentModel.DataAnnotations;

namespace MVCApplication.Models
{
    // Form cập nhật profile phía MVC; chứa cả field đọc-only và field cho phép sửa.
    public class UpdateProfileViewModel
    {
        // ========== Các field không update (chỉ để hiển thị) ==========
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? Role { get; set; }
        public bool IsBanned { get; set; }

        // ========== Các field cho phép update ==========
        [Display(Name = "Họ và tên")]
        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Họ tên phải từ 3 đến 100 ký tự")]
        public string? FullName { get; set; }

        [Display(Name = "Email")]
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? Email { get; set; }

        [Display(Name = "Số điện thoại")]
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^\d{9,11}$", ErrorMessage = "Số điện thoại phải gồm 9–11 chữ số")]
        public string? Phone { get; set; }

        [Display(Name = "Ảnh đại diện")]
        public string? AvatarUrl { get; set; }
    }
}
