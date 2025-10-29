using Microsoft.AspNetCore.Mvc;
using MVCApplication.Services.Interfaces;

namespace MVCApplication.Controllers
{
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

        // MVCApplication.Controllers/StatsController.cs

        // MVCApplication.Controllers/StatsController.cs

        [HttpGet("/api/stats/export")]
        public async Task<IActionResult> Export([FromQuery] DateTime? start, [FromQuery] DateTime? end)
        {
            var defaultStart = DateTime.Today.AddDays(-7);
            var defaultEnd = DateTime.Today.AddDays(1);

            // Lấy ngày đã nhận từ query string
            var s_received = (start ?? defaultStart).Date;
            var e_received = (end ?? defaultEnd).Date;

            // BƯỚC FIX LỖI: CỘNG THÊM 1 NGÀY VÀO CẢ START VÀ END
            // Để bù đắp cho lỗi sai lệch múi giờ/parsing khiến ngày bị lùi về 1 ngày.
            var s_fixed = s_received.AddDays(1);
            var e_fixed = e_received.AddDays(1);

            try
            {
                // GỌI API VỚI NGÀY ĐÃ ĐƯỢC FIX (+1 NGÀY)
                var data = await _stats.GetForExportAsync(s_fixed, e_fixed);

                var totalQuantity = data.Sum(x => x.Quantity);
                var totalRevenue = data.Sum(x => x.Revenue);

                // NGÀY HIỂN THỊ (s_fixed là ngày bắt đầu chính xác, e_fixed là ngày sau ngày kết thúc)
                var displayStart = s_fixed.Date;
                var displayEnd = e_fixed.Date.AddDays(-1);

                // Định dạng ngày theo dd/MM/yyyy
                var startDateStr = displayStart.ToString("dd/MM/yyyy");
                var endDateStr = displayEnd.ToString("dd/MM/yyyy");

                // Tạo nội dung CSV
                var csv = new System.Text.StringBuilder();

                // THÊM THÔNG TIN NGÀY VÀO CSV (Mỗi thông tin 1 dòng)
                csv.AppendLine($"Ngày Bắt Đầu: {startDateStr}");
                csv.AppendLine($"Ngày Kết Thúc: {endDateStr}");
                csv.AppendLine("");

                // Tiêu đề cột
                csv.AppendLine("ID Sản Phẩm,Tên Sản Phẩm,Số Lượng,Doanh Thu (VNĐ)");

                // Dữ liệu chi tiết
                foreach (var item in data)
                {
                    csv.AppendLine($"\"{item.ProductId}\",\"{item.ProductName}\",\"{item.Quantity}\",\"{item.Revenue}\"");
                }

                // Thêm dòng tổng cộng
                csv.AppendLine(" ,Tổng Cộng," + totalQuantity + "," + totalRevenue);

                // Tạo mã hóa UTF-8 có BOM
                var encoding = new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
                var bytes = encoding.GetBytes(csv.ToString());

                // Đặt tên file sử dụng ngày Bắt đầu và ngày Kết thúc đã được chuẩn hóa
                return File(bytes,
                            "text/csv",
                            $"DoanhThuSanPham_{displayStart:yyyyMMdd}_{displayEnd:yyyyMMdd}.csv");
            }
            catch (HttpRequestException ex) { return StatusCode(502, new { message = "OrderAPI error", detail = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Unexpected server error", detail = ex.Message }); }
        }
    }
}
