using AutoMapper;
using OrderAPI.DTOs;
using OrderAPI.Models;
using OrderAPI.Repositories.Interfaces;

namespace OrderAPI.Services.Interfaces
{
    public interface IStatsService
    {
        Task<SummaryDto> GetSummaryAsync(DateTime start, DateTime end);
        Task<List<RevenueByCategoryDto>> GetRevenueByCategoryAsync(DateTime start, DateTime end);
        Task<List<RevenueByProductDto>> GetRevenueByProductAsync(DateTime start, DateTime end, int top);
        Task<List<SeriesPointDto>> GetSeriesAsync(DateTime start, DateTime end, string granularity);
        Task<List<RevenueByProductDto>> GetAllRevenueByProductAsync(DateTime start, DateTime end);
    }
}

