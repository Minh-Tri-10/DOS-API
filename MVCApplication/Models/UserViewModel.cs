namespace MVCApplication.Models
{
    // ViewModel chung cho dữ liệu người dùng mà MVC sử dụng ở nhiều trang.
    public class UserViewModel
    {
        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }      // ➕ Thêm dòng này
        public string Role { get; set; } = null!;
        public bool IsBanned { get; set; }

        public string? AvatarUrl { get; set; }
    }
}
