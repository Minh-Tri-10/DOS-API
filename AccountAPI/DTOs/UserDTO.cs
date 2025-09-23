namespace AccountAPI.DTOs
{
    // DTOs/UserDTO.cs
    public class UserDTO
    {
        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }        // ✅ phải có
        public string Role { get; set; } = null!;
        public bool? IsBanned { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }


}
