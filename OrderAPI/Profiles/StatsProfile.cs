using AutoMapper;
using OrderAPI.DTOs;

namespace OrderAPI.Profiles
{
    internal record SummaryRow(int TotalOrders, int PaidOrders, int CancelledOrders, decimal Revenue, decimal AvgOrderValue);
    internal record RevenueByCategoryRow(int CategoryId, string CategoryName, decimal Revenue);
    internal record RevenueByProductRow(int ProductId, string ProductName, int Quantity, decimal Revenue);
    internal record SeriesPointRow(System.DateTime Bucket, decimal Revenue, int Orders);

    public class StatsProfile : Profile
    {
        public StatsProfile()
        {
            CreateMap<SummaryRow, SummaryDto>();
            CreateMap<RevenueByCategoryRow, RevenueByCategoryDto>();
            CreateMap<RevenueByProductRow, RevenueByProductDto>();
            CreateMap<SeriesPointRow, SeriesPointDto>();
        }
    }
}
