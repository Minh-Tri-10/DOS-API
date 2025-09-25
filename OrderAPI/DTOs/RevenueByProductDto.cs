namespace OrderAPI.DTOs
{
    public sealed class RevenueByProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public int Quantity { get; set; }
        public decimal Revenue { get; set; }
    }
}
