using System.ComponentModel.DataAnnotations;

namespace MVCApplication.Models
{
    // ViewModel dùng cho form đăng nhập trên MVC.
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
