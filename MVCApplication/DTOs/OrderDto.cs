namespace MVCApplication.DTOs
{
    public class OrderDto
    {
        public int OrderId { get; set; }
        public string? FullName { get; set; } // lấy từ UserService
        public string? OrderStatus { get; set; }
        public string? PaymentStatus { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public int UserId { get; set; }
        public string? CancelReason { get; set; }
        // Danh sách items
        public List<OrderItemDto>? Items { get; set; }
    }
}
