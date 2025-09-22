using System.ComponentModel.DataAnnotations;

namespace AccountAPI.DTOs
{
    namespace AccountAPI.DTOs
    {
        public class ChangePasswordDTO
        {
            [Required] public int UserId { get; set; }
            [Required] public string CurrentPassword { get; set; } = null!;
            [Required, MinLength(6)] public string NewPassword { get; set; } = null!;
        }
    }
}