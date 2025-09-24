using AutoMapper;
using CartAPI.DTOs;
using CartAPI.Models;
using CartAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CartAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly IMapper _mapper;

        public CartController(ICartService cartService, IMapper mapper)
        {
            _cartService = cartService;
            _mapper = mapper;
        }

        // GET: api/Cart/user/1
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<CartDTO>> GetCartByUserId(int userId)
        {
            var cart = await _cartService.GetUserCartAsync(userId);
            if (cart == null)
                return NotFound(new { Message = "Cart not found for user." });

            var dto = _mapper.Map<CartDTO>(cart);
            return Ok(dto);
        }

        // POST: api/Cart/add
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart(int cartId, int productId, int quantity)
        {
            try
            {
                await _cartService.AddItemToCartAsync(cartId, productId, quantity);
                return Ok(new { Message = "Product added successfully!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = $"Failed to add product: {ex.Message}" });
            }
        }

        // PUT: api/Cart/update/5
        [HttpPut("update/{cartItemId}")]
        public async Task<IActionResult> UpdateItemQuantity(int cartItemId, int quantity)
        {
            try
            {
                await _cartService.UpdateItemQuantityAsync(cartItemId, quantity);
                return Ok(new { Message = "Cart item updated successfully!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = $"Failed to update cart item: {ex.Message}" });
            }
        }

        // DELETE: api/Cart/remove/5
        [HttpDelete("remove/{cartItemId}")]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            try
            {
                await _cartService.RemoveItemFromCartAsync(cartItemId);
                return Ok(new { Message = "Cart item removed successfully!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = $"Failed to remove cart item: {ex.Message}" });
            }
        }

        // DELETE: api/Cart/delete/1
        [HttpDelete("delete/{cartId}")]
        public async Task<IActionResult> DeleteCart(int cartId)
        {
            try
            {
                await _cartService.DeleteCartAsync(cartId);
                return Ok(new { Message = "Cart deleted successfully!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = $"Failed to delete cart: {ex.Message}" });
            }
        }
    }
}
