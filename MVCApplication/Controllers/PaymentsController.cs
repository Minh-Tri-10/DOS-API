using Microsoft.AspNetCore.Mvc;
using MVCApplication.DTOs;
using MVCApplication.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace MVCApplication.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;


        public PaymentsController(IPaymentService paymentService, IOrderService orderService)
        {
            _paymentService = paymentService;
            _orderService = orderService;
        }

        // GET: /Payments
        public async Task<IActionResult> Index()
        {
            var payments = await _paymentService.GetAllAsync();
            return View(payments);
        }

        // GET: /Payments/Detail/5
        public async Task<IActionResult> Detail(int id)
        {
            var payment = await _paymentService.GetByIdAsync(id);
            if (payment == null)
            {
                return NotFound();
            }
            return View(payment);
        }

        // GET: /Payments/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Payments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PaymentRequestDTO dto)
        {
            if (ModelState.IsValid)
            {
                await _paymentService.CreateAsync(dto);
                return RedirectToAction(nameof(Index));
            }
            return View(dto);
        }

        // GET: /Payments/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var payment = await _paymentService.GetByIdAsync(id);
            if (payment == null)
            {
                return NotFound();
            }

            // map PaymentResponseDTO -> PaymentRequestDTO để edit
            var dto = new PaymentRequestDTO
            {
                OrderID = payment.OrderID,
                PaidAmount = payment.PaidAmount,
                PaymentMethod = payment.PaymentMethod,
                PaymentStatus = payment.PaymentStatus,
            };

            return View(dto);
        }

        // POST: /Payments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PaymentRequestDTO dto)
        {
            if (ModelState.IsValid)
            {
                await _paymentService.UpdateAsync(id, dto);
                TempData["SuccessMessage"] = "Cập nhật Thanh toán thành công!";
                return RedirectToAction(nameof(Edit), new { id = id });
            }
            return View(dto);
        }

        // POST: /Payments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _paymentService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        //GET: Paymets/Result
        public IActionResult Result(string status, int id)
        {
            ViewBag.Status = status;
            ViewBag.PaymentId = id;

            return View();
        }


        // GET: /Payments/PaymentsByOrder?orderId=123
        [HttpGet]
        public async Task<IActionResult> PaymentsByOrder(int orderId)
        {
            var payments = await _paymentService.GetPaymentsByOrderIdAsync(orderId);

            // Gọi sang service/lớp khác để lấy tổng tiền đơn hàng
            var order = await _orderService.GetByIdAsync(orderId);

            ViewBag.OrderId = orderId;
            ViewBag.TotalAmount = order?.TotalAmount ?? 0;

            return View(payments);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> VnPayCallback(string status, int orderId, int paymentId)
        {
            if (string.IsNullOrWhiteSpace(status) || orderId <= 0)
            {
                TempData["Error"] = "Ket qua thanh toan khong hop le.";
                return RedirectToAction("Index", "Orders");
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var currentUserId))
            {
                TempData["Error"] = "Phien dang nhap het han. Vui long dang nhap lai.";
                return RedirectToAction("Login", "Accounts");
            }

            var order = await _orderService.GetByIdAsync(orderId);
            if (order == null || order.UserId != currentUserId)
            {
                TempData["Error"] = "Khong tim thay don hang hoac khong thuoc quyen cua ban.";
                return RedirectToAction("Index", "Orders");
            }

            if (status.Equals("Success", StringComparison.OrdinalIgnoreCase))
            {
                var updated = await _orderService.MarkAsPaidAsync(orderId);
                if (updated)
                {
                    TempData["Success"] = "Thanh toan VNPay thanh cong! Don hang da duoc cap nhat.";
                }
                else
                {
                    TempData["Error"] = "Thanh toan VNPay thanh cong nhung khong the cap nhat trang thai don hang.";
                }
            }
            else
            {
                TempData["Error"] = "Thanh toan VNPay khong thanh cong. Vui long thu lai.";
            }

            return RedirectToAction("Index", "Orders");
        }

    }
}
