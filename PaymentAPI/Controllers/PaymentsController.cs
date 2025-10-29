using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PaymentAPI.DTOs;
using PaymentAPI.Services.Interfaces;
using PaymentAPI.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // 🔹 Bật bảo vệ JWT cho toàn controller
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _service;
        private readonly IConfiguration _configuration;

        public PaymentsController(IPaymentService service, IConfiguration configuration)
        {
            _service = service;
            _configuration = configuration;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")] // 🔹 Chỉ Admin mới xem tất cả payments
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var payment = await _service.GetByIdAsync(id);
            if (payment == null) return NotFound();
            return Ok(payment);
        }

        //Create payment
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] PaymentRequestDTO req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

<<<<<<< HEAD
            if (dto.PaymentMethod?.Equals("VNPay", StringComparison.OrdinalIgnoreCase) == true)
            {
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
                var vnpUrl = await _service.CreateVnPayPaymentAsync(dto, ip);
                return Ok(new { paymentUrl = vnpUrl });
            }
            else if (dto.PaymentMethod?.Equals("COD", StringComparison.OrdinalIgnoreCase) == true)
            {
                var created = await _service.CreatePaymentAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.PaymentId }, created);
            }
            else
            {
                return BadRequest("Unsupported payment method. Only 'COD' and 'VNPay' are supported.");
=======
            switch (req.PaymentMethod?.ToUpper())
            {
                case "COD":
                    {
                        var r = await _service.CreatePaymentAsync(req);
                        return CreatedAtAction(nameof(GetById), new { id = r.PaymentId }, r);
                    }

                case "VNPAY":
                    {
                        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
                        var r = await _service.CreateVnPayPaymentAsync(req, ip);
                        return Ok(r);
                    }

                default:
                    return BadRequest("Unsupported payment method");
>>>>>>> 438bfee (Minor change in code format)
            }
        }


        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] PaymentUpdateDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var updated = await _service.UpdateAsync(id, dto);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var removed = await _service.DeleteAsync(id);
            return Ok(removed);
        }

        [HttpPost("{id}/confirm")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Confirm(int id, [FromQuery] string status)
        {
            var result = await _service.ConfirmPaymentAsync(id, status);
            return Ok(result);
        }

<<<<<<< HEAD
        // -------------------- VNPay Integration --------------------

=======
        // VNPAY sẽ redirect user -> GET with query params
>>>>>>> 438bfee (Minor change in code format)
        [HttpGet("vnpay-return")]
        [AllowAnonymous]
        public async Task<IActionResult> VnPayReturn()
        {
            var query = Request.Query.ToDictionary(k => k.Key, v => v.Value.ToString());
            var cfg = _configuration.GetSection("Vnpay");
            var secret = cfg["HashSecret"];

            if (!VnPayHelper.VerifySignature(query, secret))
                return BadRequest("Invalid signature");

            var txnRef = query.ContainsKey("vnp_TxnRef") ? query["vnp_TxnRef"] : null;
            var vnpResponseCode = query.ContainsKey("vnp_ResponseCode") ? query["vnp_ResponseCode"] : null;
            var vnpTransNo = query.ContainsKey("vnp_TransactionNo") ? query["vnp_TransactionNo"] : null;

            if (!int.TryParse(txnRef, out var paymentId))
                return BadRequest("Invalid txnRef");

            if (vnpResponseCode == "00")
            {
                await _service.ConfirmPaymentAsync(paymentId, "Success", vnpTransNo);
                return Redirect($"https://localhost:7223/Orders/Details/{paymentId}");
            }
            else
            {
                await _service.ConfirmPaymentAsync(paymentId, "Failed", vnpTransNo);
                return Redirect($"https://localhost:7223/Orders/Details/{paymentId}");
            }
        }

        [HttpPost("vnpay-ipn")]
        [AllowAnonymous]
        public async Task<IActionResult> VnPayIpn()
        {
            IDictionary<string, string> allParams;
            if (Request.HasFormContentType)
                allParams = Request.Form.ToDictionary(k => k.Key, v => v.Value.ToString());
            else
                allParams = Request.Query.ToDictionary(k => k.Key, v => v.Value.ToString());

            var cfg = _configuration.GetSection("Vnpay");
            var secret = cfg["HashSecret"];

            if (!VnPayHelper.VerifySignature(allParams, secret))
                return BadRequest("Invalid signature");

            var txnRef = allParams.ContainsKey("vnp_TxnRef") ? allParams["vnp_TxnRef"] : null;
            var vnpResponseCode = allParams.ContainsKey("vnp_ResponseCode") ? allParams["vnp_ResponseCode"] : null;
            var vnpTransNo = allParams.ContainsKey("vnp_TransactionNo") ? allParams["vnp_TransactionNo"] : null;

            if (!int.TryParse(txnRef, out var paymentId))
                return BadRequest("Invalid txnRef");

            if (vnpResponseCode == "00")
            {
                await _service.ConfirmPaymentAsync(paymentId, "Success", vnpTransNo);
                return Content("00");
            }
            else
            {
                await _service.ConfirmPaymentAsync(paymentId, "Failed", vnpTransNo);
                return Content("01");
            }
        }

        // -------------------- Get payments by OrderId --------------------

        [HttpGet("by-order/{orderId}")]
        public async Task<IActionResult> GetByOrderId(int orderId)
        {
            var payments = await _service.GetByOrderIdAsync(orderId);
            if (!payments.Any())
                return NotFound("Chưa có thanh toán cho đơn hàng này.");

            return Ok(payments);
        }
    }
}
