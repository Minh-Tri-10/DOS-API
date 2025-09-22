namespace MVCApplication.Models
{
    public class UserViewModel
    {
        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string Role { get; set; } = null!;
        public bool IsBanned { get; set; }
    }
}
