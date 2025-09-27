using Microsoft.AspNetCore.Mvc;
using MVCApplication.DTOs;
using MVCApplication.Services.Interfaces;

    namespace MVCApplication.Controllers
    {
        public class OrdersController : Controller
        {
            private readonly IOrderService _service;
            private const int PageSize = 5;
            private const string UserSessionKey = "UserId";

            public OrdersController(IOrderService service)
            {
                _service = service;
            }

            // Danh sách đơn hàng của user hiện tại
            public async Task<IActionResult> Index(int pageNumber = 1)
            {
                int? userId = HttpContext.Session.GetInt32(UserSessionKey);
                if (userId == null || userId == 0)
                {
                    TempData["Error"] = "Bạn cần đăng nhập để xem đơn hàng.";
                    return RedirectToAction("Login", "Accounts");
                }

                var allOrders = await _service.GetOrdersByUserIdAsync(userId.Value);

                // Phân trang
                int totalOrders = allOrders.Count;
                int totalPages = (int)Math.Ceiling(totalOrders / (double)PageSize);
                var orders = allOrders
                    .Skip((pageNumber - 1) * PageSize)
                    .Take(PageSize)
                    .ToList();

                ViewBag.CurrentPage = pageNumber;
                ViewBag.TotalPages = totalPages;

                return View(orders);
            }

            // Chi tiết đơn hàng
            public async Task<IActionResult> Details(int id)
            {
                var order = await _service.GetByIdAsync(id);
                if (order == null) return NotFound();

                // Kiểm tra quyền user xem đơn này có thuộc về họ
                int? userId = HttpContext.Session.GetInt32(UserSessionKey);
                if (userId != order.UserId) return Forbid();

                return View(order);
            }

        [HttpGet]
        public IActionResult Create()
        {
            // Lấy dữ liệu từ sessionStorage thông qua TempData hoặc JS (nếu dùng ViewModel)
            return View(); // trang hiển thị các sản phẩm đã chọn
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateOrderDto dto)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["Error"] = "Bạn cần đăng nhập.";
                return RedirectToAction("Login", "Accounts");
            }

            dto.UserId = userId.Value;

            if (dto.Items == null || !dto.Items.Any())
            {
                TempData["Error"] = "Chưa có sản phẩm để tạo đơn.";
                return RedirectToAction("Create");
            }

            var orderId = await _service.CreateAsync(dto);
            if (orderId == null)
            {
                TempData["Error"] = "Tạo đơn hàng thất bại.";
                return RedirectToAction("Create");
            }

            TempData["Success"] = "Đặt hàng thành công!";
            return RedirectToAction("Details", new { id = orderId });
        }

    }
}
