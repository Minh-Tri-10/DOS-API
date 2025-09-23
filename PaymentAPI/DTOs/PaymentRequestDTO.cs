namespace PaymentAPI.DTOs
{
    public class PaymentRequestDTO
    {
        public int OrderId { get; set; }
        public decimal PaidAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
    }

}
