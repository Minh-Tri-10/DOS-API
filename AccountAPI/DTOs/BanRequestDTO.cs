using System.ComponentModel.DataAnnotations;

namespace AccountAPI.DTOs
{

    
        // Payload nhỏ gọn để admin thiết lập trạng thái khóa tài khoản.
        public class BanRequestDTO
        {
            [Required] public bool IsBanned { get; set; }
        }
    }
