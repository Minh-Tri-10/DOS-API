using System.ComponentModel.DataAnnotations;

namespace AccountAPI.DTOs
{
    public class UpdateProfileDTO
    {
        [Required, StringLength(100)]
        public string? FullName { get; set; }

        [EmailAddress, StringLength(100)]
        public string? Email { get; set; }

        [Phone, StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(255)]
        public string? AvatarUrl { get; set; }
    }
}
