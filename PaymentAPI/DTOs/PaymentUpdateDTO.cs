namespace PaymentAPI.DTOs
{
    public class PaymentUpdateDTO
    {
        public decimal PaidAmount { get; set; }
        public string? PaymentMethod { get; set; }
        public string? PaymentStatus { get; set; }
    }
}
