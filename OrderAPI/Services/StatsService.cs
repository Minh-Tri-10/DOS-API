using Microsoft.EntityFrameworkCore;
using OrderAPI.Models;
using OrderAPI.DTOs;
using AutoMapper;
using OrderAPI.Profiles;
using OrderAPI.Services.Interfaces;
using OrderAPI.Repositories.Interfaces;

namespace OrderAPI.Services
{
    public sealed class StatsService : IStatsService
    {
        private readonly IStatsRepository _repo;

        public StatsService(IStatsRepository repo) => _repo = repo;

        public Task<SummaryDto> GetSummaryAsync(DateTime start, DateTime end)
            => _repo.GetSummaryAsync(start, end);

        public Task<List<RevenueByCategoryDto>> GetRevenueByCategoryAsync(DateTime start, DateTime end)
            => _repo.GetRevenueByCategoryAsync(start, end);

        public Task<List<RevenueByProductDto>> GetRevenueByProductAsync(DateTime start, DateTime end, int top)
            => _repo.GetRevenueByProductAsync(start, end, top);

        public Task<List<SeriesPointDto>> GetSeriesAsync(DateTime start, DateTime end, string granularity)
            => _repo.GetSeriesAsync(start, end, granularity);
        public Task<List<RevenueByProductDto>> GetAllRevenueByProductAsync(DateTime start, DateTime end)
            => _repo.GetAllRevenueByProductAsync(start, end);
    }
}
