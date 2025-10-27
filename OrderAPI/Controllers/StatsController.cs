// Controllers/StatsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderAPI.DTOs;
using OrderAPI.Services.Interfaces;

[ApiController]
[Route("api/stats")]
[Authorize(Roles = "Admin")]
public class StatsController : ControllerBase
{
    private readonly IStatsService _stats;
    public StatsController(IStatsService stats) => _stats = stats;

    [HttpGet("summary")]
    public Task<SummaryDto> Summary([FromQuery] DateTime start, [FromQuery] DateTime end)
        => _stats.GetSummaryAsync(start, end);

    [HttpGet("by-category")]
    public Task<List<RevenueByCategoryDto>> ByCategory([FromQuery] DateTime start, [FromQuery] DateTime end)
        => _stats.GetRevenueByCategoryAsync(start, end);

    [HttpGet("by-product")]
    public Task<List<RevenueByProductDto>> ByProduct([FromQuery] DateTime start, [FromQuery] DateTime end, [FromQuery] int top = 10)
        => _stats.GetRevenueByProductAsync(start, end, top);

    [HttpGet("series")]
    public Task<List<SeriesPointDto>> Series([FromQuery] DateTime start, [FromQuery] DateTime end, [FromQuery] string granularity = "day")
        => _stats.GetSeriesAsync(start, end, granularity);
}
