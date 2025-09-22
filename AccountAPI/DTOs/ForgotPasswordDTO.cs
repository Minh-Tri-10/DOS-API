using System.ComponentModel.DataAnnotations;

namespace AccountAPI.DTOs
{
    public class ResetPasswordDTO
    {
        [Required] public string Token { get; set; } = null!;
        [Required, MinLength(6)] public string NewPassword { get; set; } = null!;
    }
}
