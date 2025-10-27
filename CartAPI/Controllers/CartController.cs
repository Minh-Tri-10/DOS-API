using System.Security.Claims;
using AutoMapper;
using CartAPI.DTOs;
using CartAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CartAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _service;
        private readonly IMapper _mapper;

        public CartController(ICartService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<CartDTO>> GetCartByUserId(int userId)
        {
            if (!IsCurrentUser(userId)) return Forbid();

            var cart = await _service.GetOrCreateUserCartAsync(userId);
            return Ok(_mapper.Map<CartDTO>(cart));
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] AddCartItemDTO dto)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null) return Unauthorized();
            if (dto == null) return BadRequest(new { message = "Body is required" });
            if (dto.UserId != currentUserId.Value) return Forbid();

            var cart = await _service.GetOrCreateUserCartAsync(dto.UserId);
            await _service.AddItemToCartAsync(cart.CartId, dto.ProductId, dto.Quantity);

            return Ok(new { message = "Da them hoac cap nhat san pham vao gio!" });
        }

        [HttpPut("update/{cartItemId}")]
        public async Task<IActionResult> UpdateItemQuantity(int cartItemId, [FromQuery] int quantity)
        {
            if (GetCurrentUserId() == null) return Unauthorized();
            await _service.UpdateItemQuantityAsync(cartItemId, quantity);
            return Ok(new { message = "Cap nhat so luong thanh cong!" });
        }

        [HttpDelete("remove/{cartItemId}")]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            if (GetCurrentUserId() == null) return Unauthorized();
            await _service.RemoveItemFromCartAsync(cartItemId);
            return Ok(new { message = "Da xoa san pham khoi gio!" });
        }

        [HttpDelete("delete/{cartId}")]
        public async Task<IActionResult> DeleteCart(int cartId)
        {
            if (GetCurrentUserId() == null) return Unauthorized();
            await _service.DeleteCartAsync(cartId);
            return Ok(new { message = "Da xoa gio hang!" });
        }

        [HttpGet("count")]
        public async Task<IActionResult> Count()
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null) return Ok(new { count = 0 });

            var cart = await _service.GetOrCreateUserCartAsync(currentUserId.Value);
            return Ok(new { count = cart?.CartItems?.Count ?? 0 });
        }

        private int? GetCurrentUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claim, out var userId) ? userId : null;
        }

        private bool IsCurrentUser(int userId) => GetCurrentUserId() == userId;
    }
}
