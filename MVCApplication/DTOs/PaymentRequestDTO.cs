using System.ComponentModel.DataAnnotations;

namespace MVCApplication.DTOs
{
    public class PaymentRequestDTO
    {
        public int OrderID { get; set; }
        public decimal PaidAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentStatus { get; set; }
    }
}
