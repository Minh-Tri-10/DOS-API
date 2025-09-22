namespace AccountAPI.DTOs
{
    // DTOs/UserDTO.cs
    public class UserDTO
    {
        public int UserId { get; set; }                
        public string Username { get; set; } = null!;
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string Role { get; set; } = null!;
        public bool IsBanned { get; set; }              // <- non-nullable để FE dùng dễ
        public DateTime CreatedAt { get; set; }
    }

}
