// Controllers/CartController.cs
using AutoMapper;
using CartAPI.DTOs;
using CartAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CartAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICartService _service;
        private readonly IMapper _mapper;

        public CartController(ICartService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        // GET: api/Cart/user/4
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<CartDTO>> GetCartByUserId(int userId)
        {
            var cart = await _service.GetOrCreateUserCartAsync(userId);
            return Ok(_mapper.Map<CartDTO>(cart));
        }

        // POST: api/Cart/add
        // Body: { "userId": 4, "productId": 1, "quantity": 1 }
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] AddCartItemDTO dto)
        {
            if (dto == null) return BadRequest(new { message = "Body is required" });

            var cart = await _service.GetOrCreateUserCartAsync(dto.UserId);
            await _service.AddItemToCartAsync(cart.CartId, dto.ProductId, dto.Quantity);

            return Ok(new { message = "Đã thêm/cộng dồn sản phẩm vào giỏ!" });
        }

        // PUT: api/Cart/update/21?quantity=3
        [HttpPut("update/{cartItemId}")]
        public async Task<IActionResult> UpdateItemQuantity(int cartItemId, [FromQuery] int quantity)
        {
            await _service.UpdateItemQuantityAsync(cartItemId, quantity);
            return Ok(new { message = "Cập nhật số lượng thành công!" });
        }

        // DELETE: api/Cart/remove/21
        [HttpDelete("remove/{cartItemId}")]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            await _service.RemoveItemFromCartAsync(cartItemId);
            return Ok(new { message = "Đã xoá sản phẩm khỏi giỏ!" });
        }

        // DELETE: api/Cart/delete/3
        [HttpDelete("delete/{cartId}")]
        public async Task<IActionResult> DeleteCart(int cartId)
        {
            await _service.DeleteCartAsync(cartId);
            return Ok(new { message = "Đã xoá giỏ hàng!" });
        }
    }
}
