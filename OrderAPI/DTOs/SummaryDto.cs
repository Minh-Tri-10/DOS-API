namespace OrderAPI.DTOs
{
    public sealed class SummaryDto
    {
        public int TotalOrders { get; set; }
        public int PaidOrders { get; set; }
        public int CancelledOrders { get; set; }
        public decimal Revenue { get; set; }
        public decimal AvgOrderValue { get; set; }
    }

}
