using System.ComponentModel.DataAnnotations;

namespace MVCApplication.DTOs
{
    public class FeedbackRequestDTO
    {
        [Required(ErrorMessage = "Order ID is required.")]
        public int OrderId { get; set; }

        [Required(ErrorMessage = "Yêu cầu đánh giá")]
        [Range(1, 5, ErrorMessage = "Đánh giá phải từ 1 đến 5")]
        public int Rating { get; set; }

        [MaxLength(500, ErrorMessage = "Bình luận không thể vượt quá 500 ký tự")]
        public string? Comment { get; set; }
    }
}
