using AccountAPI.Validations;
using System.ComponentModel.DataAnnotations;

namespace AccountAPI.DTOs
{
    //[UniqueEmailOrPhone]
    public class UpdateProfileDTO
    {
        public int UserId { get; set; } // cần để validation biết user hiện tại
        [Required, StringLength(100)]
        public string? FullName { get; set; }

        [EmailAddress, StringLength(100)]
        [UniqueEmail] // ✅ Custom Validation Attribute
        public string? Email { get; set; }

        [Phone, StringLength(20)]
        [UniquePhone] // ✅ Custom Validation Attribute
        public string? Phone { get; set; }

        [StringLength(255)]
        public string? AvatarUrl { get; set; }
        public IFormFile? AvatarFile { get; set; }
    }
}
