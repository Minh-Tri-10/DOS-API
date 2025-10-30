using OrderAPI.DTOs;
using OrderAPI.Models;

namespace OrderAPI.Repositories.Interfaces
{
    public interface IStatsRepository
    {
        Task<SummaryDto> GetSummaryAsync(DateTime start, DateTime end);
        Task<List<RevenueByCategoryDto>> GetRevenueByCategoryAsync(DateTime start, DateTime end);
        Task<List<RevenueByProductDto>> GetRevenueByProductAsync(DateTime start, DateTime end, int top);
        Task<List<SeriesPointDto>> GetSeriesAsync(DateTime start, DateTime end, string granularity);
        Task<List<RevenueByProductDto>> GetAllRevenueByProductAsync(DateTime start, DateTime end);
    }
}
