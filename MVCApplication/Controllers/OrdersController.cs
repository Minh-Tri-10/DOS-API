using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVCApplication.DTOs;
using MVCApplication.Services.Interfaces;

namespace MVCApplication.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly IOrderService _service;
        private const int PageSize = 5;

        public OrdersController(IOrderService service)
        {
            _service = service;
        }

        private int? CurrentUserId
        {
            get
            {
                var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                return int.TryParse(claim, out var id) ? id : null;
            }
        }

        public async Task<IActionResult> Index(int pageNumber = 1)
        {
            if (CurrentUserId == null)
            {
                TempData["Error"] = "Ban can dang nhap de xem don hang.";
                return RedirectToAction("Login", "Accounts");
            }

            var allOrders = await _service.GetOrdersByUserIdAsync(CurrentUserId.Value);
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

        public async Task<IActionResult> Details(int id)
        {
            var order = await _service.GetByIdAsync(id);
            if (order == null) return NotFound();

            if (CurrentUserId != order.UserId) return Forbid();

            return View(order);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateOrderDto dto)
        {
            if (CurrentUserId == null)
            {
                TempData["Error"] = "Ban can dang nhap.";
                return RedirectToAction("Login", "Accounts");
            }

            dto.UserId = CurrentUserId.Value;

            if (dto.Items == null || !dto.Items.Any())
            {
                TempData["Error"] = "Chua co san pham de tao don.";
                return RedirectToAction("Create");
            }

            var orderId = await _service.CreateAsync(dto);
            if (orderId == null)
            {
                TempData["Error"] = "Tao don hang that bai.";
                return RedirectToAction("Create");
            }

            TempData["Success"] = "Dat hang thanh cong!";
            return RedirectToAction("Details", new { id = orderId });
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int id, string reason = "Nguoi dung huy don")
        {
            if (CurrentUserId == null) return Forbid();

            var success = await _service.CancelAsync(id, reason);
            if (!success)
            {
                TempData["Error"] = "Huy don that bai.";
            }
            else
            {
                TempData["Success"] = "Don hang da duoc huy.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
