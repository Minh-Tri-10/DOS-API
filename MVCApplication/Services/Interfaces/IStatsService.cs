using MVCApplication.DTOs;
using MVCApplication.Models;

namespace MVCApplication.Services.Interfaces
{
    public interface IStatsService
    {
        Task<SummaryDto> GetSummaryAsync(DateTime start, DateTime end);
        Task<List<RevenueByCategoryDto>> GetByCategoryAsync(DateTime start, DateTime end);
        Task<List<RevenueByProductDto>> GetByProductAsync(DateTime start, DateTime end, int top = 10);
        Task<List<SeriesPointDto>> GetSeriesAsync(DateTime start, DateTime end, string granularity = "day");
        Task<List<RevenueByProductDto>> GetForExportAsync(DateTime start, DateTime end);
    }
}
