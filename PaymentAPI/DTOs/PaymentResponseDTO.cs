namespace PaymentAPI.DTOs
{
    public class PaymentResponseDTO
    {
        public int PaymentId { get; set; }
        public decimal PaidAmount { get; set; }
        public string? PaymentUrl { get; set; }
    }


}
