namespace MVCApplication.DTOs
{
    public class PaymentResponseDTO
    {
        public int PaymentID { get; set; }
        public int OrderID { get; set; }
        public decimal PaidAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime? PaymentDate { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
    }
}
