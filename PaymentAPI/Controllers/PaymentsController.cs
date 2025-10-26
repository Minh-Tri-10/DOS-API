using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentAPI.DTOs;
using PaymentAPI.Models;
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
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var p = await _service.GetByIdAsync(id);
            if (p == null) return NotFound();
            return Ok(p);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PaymentRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var r = await _service.CreatePaymentAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = r.PaymentId }, r);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PaymentUpdateDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var r = await _service.UpdateAsync(id, dto);
            return Ok(r);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var r = await _service.DeleteAsync(id);
            return Ok(r);
        }

        // optional: endpoint để confirm (manual) khi tạm thời không tích hợp cổng online
        [HttpPost("{id}/confirm")]
        public async Task<IActionResult> Confirm(int id, [FromQuery] string status)
        {
            var r = await _service.ConfirmPaymentAsync(id, status);
            return Ok(r);
        }

        [HttpPost("create-vnpay")]
        public async Task<IActionResult> CreateVnPay([FromBody] PaymentRequestDTO request)
        {
            // lấy IP client
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var res = await _service.CreateVnPayPaymentAsync(request, ip ?? "127.0.0.1");
            return Ok(res);
        }

        // VNPAY sẽ redirect user -> GET with query params
        [HttpGet("vnpay-return")]
        public async Task<IActionResult> VnPayReturn()
        {
            var query = Request.Query.ToDictionary(k => k.Key, v => v.Value.ToString());
            var cfg = _configuration.GetSection("Vnpay");
            var secret = cfg["HashSecret"];

            if (!VnPayHelper.VerifySignature(query, secret))
                return BadRequest("Invalid signature");

            // read params
            var txnRef = query.ContainsKey("vnp_TxnRef") ? query["vnp_TxnRef"] : null;
            var vnpResponseCode = query.ContainsKey("vnp_ResponseCode") ? query["vnp_ResponseCode"] : null;
            var vnpTransNo = query.ContainsKey("vnp_TransactionNo") ? query["vnp_TransactionNo"] : null;

            if (!int.TryParse(txnRef, out var paymentId))
                return BadRequest("Invalid txnRef");

            if (vnpResponseCode == "00")
            {
                await _service.ConfirmPaymentAsync(paymentId, "Success", vnpTransNo);
                return Redirect($"https://localhost:7223/Payments/Result?status=success&id={paymentId}");
            }
            else
            {
                await _service.ConfirmPaymentAsync(paymentId, "Failed", vnpTransNo);
                return Redirect($"https://localhost:7223/Payments/Result?status=failed&id={paymentId}");
            }

        }

        // IPN (server -> server). VNPAY may call as GET or POST depending on config.
        // return "00" (or appropriate code) when you processed successfully (see spec).
        [HttpPost("vnpay-ipn")]
        public async Task<IActionResult> VnPayIpn()
        {
            // read form or query
            IDictionary<string, string> allParams;
            if (Request.HasFormContentType)
            {
                allParams = Request.Form.ToDictionary(k => k.Key, v => v.Value.ToString());
            }
            else
            {
                allParams = Request.Query.ToDictionary(k => k.Key, v => v.Value.ToString());
            }

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
                // theo spec VNPAY mong đợi merchant trả về "00" (giao dịch đã được merchant ghi nhận).
                // Đọc spec: merchant trả về code 00 = ghi nhận thành công.
                return Content("00");
            }
            else
            {
                await _service.ConfirmPaymentAsync(paymentId, "Failed", vnpTransNo);
                return Content("01"); // hoặc thông báo lỗi
            }
        }

    }
}
