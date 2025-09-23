namespace OrderAPI.DTOs
{
    public class UpdateOrderDto
    {
        public string? OrderStatus { get; set; }
        public string? PaymentStatus { get; set; }
        public string? CancelReason { get; set; }
    }
}
