using System.Text.Json;
using MVCApplication.DTOs;
using MVCApplication.Services.Interfaces;

namespace MVCApplication.Services
{
    public class StatsService : IStatsService
    {
        private readonly IHttpClientFactory _httpFactory;
        private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };
        public StatsService(IHttpClientFactory httpFactory) => _httpFactory = httpFactory;
        private HttpClient Api => _httpFactory.CreateClient("OrderApi");

        private async Task<T> GetJsonAsync<T>(string path)
        {
            using var resp = await Api.GetAsync(path);
            var text = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
                throw new HttpRequestException($"OrderAPI {(int)resp.StatusCode} {resp.ReasonPhrase}: {text}");
            return System.Text.Json.JsonSerializer.Deserialize<T>(text, _json)!;
        }

        public Task<SummaryDto> GetSummaryAsync(DateTime start, DateTime end) =>
            GetJsonAsync<SummaryDto>($"/api/stats/summary?start={start:O}&end={end:O}");

        public Task<List<RevenueByCategoryDto>> GetByCategoryAsync(DateTime start, DateTime end) =>
            GetJsonAsync<List<RevenueByCategoryDto>>($"/api/stats/by-category?start={start:O}&end={end:O}");

        public Task<List<RevenueByProductDto>> GetByProductAsync(DateTime start, DateTime end, int top = 10) =>
            GetJsonAsync<List<RevenueByProductDto>>($"/api/stats/by-product?start={start:O}&end={end:O}&top={top}");

        public Task<List<SeriesPointDto>> GetSeriesAsync(DateTime start, DateTime end, string granularity = "day") =>
            GetJsonAsync<List<SeriesPointDto>>($"/api/stats/series?start={start:O}&end={end:O}&granularity={granularity}");
    }
}
