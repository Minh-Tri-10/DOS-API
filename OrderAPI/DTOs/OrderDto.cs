using System.ComponentModel.DataAnnotations;

namespace OrderAPI.DTOs
{
    public class OrderDto
    {
        [Key]
        public int OrderId { get; set; }
        public string? FullName { get; set; } // lấy từ UserService
        public string? OrderStatus { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public int UserId { get; set; }
        // Thêm cancel reason
        public string? CancelReason { get; set; }
        // Danh sách items
        public List<OrderItemDto>? Items { get; set; }
    }
}
