using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentAPI.DTOs;
using PaymentAPI.Models;
using PaymentAPI.Services.Interfaces;
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
        public PaymentsController(IPaymentService service) => _service = service;

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
    }
}
