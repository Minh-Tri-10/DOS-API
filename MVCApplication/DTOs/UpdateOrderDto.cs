using System.ComponentModel.DataAnnotations;

namespace MVCApplication.DTOs
{
    public class UpdateOrderDto
    {
        [Required]
        [Display(Name = "Order Status")]
        public string? OrderStatus { get; set; }

        [Display(Name = "Cancel Reason")]
        public string? CancelReason { get; set; }
    }
}
