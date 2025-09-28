using Microsoft.AspNetCore.Mvc;
using MVCApplication.DTOs;
using MVCApplication.Services;
using MVCApplication.Services.Interfaces;

namespace MVCApplication.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
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
                return RedirectToAction(nameof(Index));
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
    }
}
