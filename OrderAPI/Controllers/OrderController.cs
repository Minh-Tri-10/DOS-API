using Microsoft.AspNetCore.Mvc;
using OrderAPI.DTOs;
using OrderAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace OrderAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _service;

        public OrderController(IOrderService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            (List<OrderDto> orders, int totalCount) = await _service.GetPagedAsync(page, pageSize);

            var result = new
            {
                data = orders,
                totalCount,
                page,
                pageSize
            };

            return Ok(result);
        }



        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var order = await _service.GetByIdAsync(id);
            if (order == null) return NotFound();
            return Ok(order);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
        {
            var orderId = await _service.CreateAsync(dto);
            var order = await _service.GetByIdAsync(orderId);

            return CreatedAtAction(nameof(Get), new { id = orderId }, order);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateOrderDto dto)
        {
            await _service.UpdateAsync(id, dto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            var orders = await _service.GetOrdersByUserIdAsync(userId);
            return Ok(orders);
        }

        [HttpPut("{id}/pay")]
        public async Task<IActionResult> MarkAsPaid(int id)
        {
            await _service.MarkAsPaidAsync(id);
            return NoContent();
        }
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(int id, [FromBody] string cancelReason)
        {
            var success = await _service.CancelAsync(id, cancelReason);
            if (!success) return NotFound("Order not found or already cancelled");

            return NoContent();
        }

    }
}
