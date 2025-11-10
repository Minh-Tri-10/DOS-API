using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MVCApplication.Services.Interfaces;

namespace MVCApplication.Controllers
{
    [Authorize(Roles = "Admin")]
    public class StatsController : Controller
    {
        private readonly IStatsService _stats;
        public StatsController(IStatsService stats) => _stats = stats;

        [HttpGet("/Stats")]
        [HttpGet("/Stats/Index")]
        public IActionResult Index() => View();

        [HttpGet("/api/stats/summary")]
        public async Task<IActionResult> Summary([FromQuery] DateTime? start, [FromQuery] DateTime? end)
        {
            var s = start ?? DateTime.Today.AddDays(-7);
            var e = end ?? DateTime.Today.AddDays(1);
            try { return Json(await _stats.GetSummaryAsync(s, e)); }
            catch (HttpRequestException ex) { return StatusCode(502, new { message = "OrderAPI error", detail = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Unexpected server error", detail = ex.Message }); }
        }

        [HttpGet("/api/stats/by-category")]
        public async Task<IActionResult> ByCategory([FromQuery] DateTime? start, [FromQuery] DateTime? end)
        {
            var s = start ?? DateTime.Today.AddDays(-7);
            var e = end ?? DateTime.Today.AddDays(1);
            try { return Json(await _stats.GetByCategoryAsync(s, e)); }
            catch (HttpRequestException ex) { return StatusCode(502, new { message = "OrderAPI error", detail = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Unexpected server error", detail = ex.Message }); }
        }

        [HttpGet("/api/stats/by-product")]
        public async Task<IActionResult> ByProduct([FromQuery] DateTime? start, [FromQuery] DateTime? end, [FromQuery] int top = 10)
        {
            var s = start ?? DateTime.Today.AddDays(-7);
            var e = end ?? DateTime.Today.AddDays(1);
            try { return Json(await _stats.GetByProductAsync(s, e, top)); }
            catch (HttpRequestException ex) { return StatusCode(502, new { message = "OrderAPI error", detail = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Unexpected server error", detail = ex.Message }); }
        }

        [HttpGet("/api/stats/series")]
        public async Task<IActionResult> Series([FromQuery] DateTime? start, [FromQuery] DateTime? end, [FromQuery] string granularity = "day")
        {
            var s = start ?? DateTime.Today.AddDays(-7);
            var e = end ?? DateTime.Today.AddDays(1);
            try { return Json(await _stats.GetSeriesAsync(s, e, granularity)); }
            catch (HttpRequestException ex) { return StatusCode(502, new { message = "OrderAPI error", detail = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Unexpected server error", detail = ex.Message }); }
        }
    }
}
