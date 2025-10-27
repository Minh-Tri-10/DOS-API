using Microsoft.AspNetCore.Mvc;
using MVCApplication.DTOs;
using MVCApplication.Services.Interfaces;

namespace MVCApplication.Controllers
{
    public class AdminOrdersController : Controller
    {
        private readonly IOrderService _service;

        public AdminOrdersController(IOrderService service)
        {
            _service = service;
        }

        // Danh sách đơn hàng
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            var result = await _service.GetPagedAsync(page, pageSize);

            var orders = result?.Data ?? new List<OrderDto>();
            ViewBag.TotalCount = result?.TotalCount ?? 0;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;

            return View(orders);
        }


        // Chi tiết đơn hàng
        public async Task<IActionResult> Details(int id)
        {
            var order = await _service.GetByIdAsync(id);
            if (order == null) return NotFound();
            return View(order);
        }

        // Tạo đơn hàng
        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(CreateOrderDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var orderId = await _service.CreateAsync(dto);
            if (orderId == null)
            {
                ViewBag.Error = "Tạo đơn hàng thất bại";
                return View(dto);
            }

            return RedirectToAction(nameof(Details), new { id = orderId });
        }

        // Cập nhật trạng thái đơn hàng
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var order = await _service.GetByIdAsync(id);
            if (order == null) return NotFound();

            var dto = new UpdateOrderDto
            {
                OrderStatus = order.OrderStatus,
                PaymentStatus = order.PaymentStatus
            };

            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, UpdateOrderDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var ok = await _service.UpdateAsync(id, dto);
            if (!ok)
            {
                ViewBag.Error = "Cập nhật thất bại";
                return View(dto);
            }

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
