﻿using System;
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
        private readonly IPaymentService _paymentService;
        private const int PageSize = 5;

        public OrdersController(IOrderService service, IPaymentService paymentService)
        {
            _service = service;
            _paymentService = paymentService;
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
                TempData["Error"] = "Bạn cần đăng nhập.";
                return RedirectToAction("Login", "Accounts");
            }

            dto.UserId = CurrentUserId.Value;

            if (dto.Items == null || !dto.Items.Any())
            {
                TempData["Error"] = "Chưa có sản phẩm để tạo đơn.";
                return RedirectToAction("Create");
            }

            var paymentMethod = Request.Form["PaymentMethod"].ToString();

            // 1️⃣ Tạo đơn hàng
            var orderId = await _service.CreateAsync(dto);
            if (orderId == null)
            {
                TempData["Error"] = "Tạo đơn hàng thất bại.";
                return RedirectToAction("Create");
            }

            // 2️⃣ Tạo thanh toán
            var totalAmount = dto.Items.Sum(i => i.Quantity * i.UnitPrice) + 30000;
            var paymentDto = new PaymentRequestDTO
            {
                OrderID = orderId.Value,
                PaymentMethod = paymentMethod,
                PaidAmount = totalAmount,
                PaymentStatus = "Pending"
            };

            var paymentUrl = await _paymentService.CreateAsync(paymentDto);

            // 3️⃣ Nếu có URL trả về (VNPay) => redirect sang VNPay
            if (!string.IsNullOrEmpty(paymentUrl))
            {
                Console.WriteLine($"Payment URL: {paymentUrl}");
                return Redirect(paymentUrl);
            }


            // 4️⃣ COD => về trang chi tiết
            TempData["Success"] = "Đặt hàng thành công!";
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
