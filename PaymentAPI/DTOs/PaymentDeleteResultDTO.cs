namespace PaymentAPI.DTOs
{
    public class PaymentDeleteResultDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
