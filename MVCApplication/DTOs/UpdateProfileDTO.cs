using System.ComponentModel.DataAnnotations;

namespace MVCApplication.DTOs
{
    public class UpdateProfileDTO
    {
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
