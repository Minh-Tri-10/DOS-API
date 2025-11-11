using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVCApplication.DTOs;
using MVCApplication.Services.Interfaces;

namespace MVCApplication.Controllers
{
    [Authorize]
    [Route("api/cart")]
    public class CartController : Controller
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        private int? CurrentUserId
        {
            get
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                return int.TryParse(userIdClaim, out var userId) ? userId : null;
            }
        }

        [HttpGet("/Cart")]
        public async Task<IActionResult> Index()
        {
            if (CurrentUserId == null) return RedirectToAction("Login", "Accounts");
            var cart = await _cartService.GetCartWithProductsAsync(CurrentUserId.Value);
            return View(cart);
        }

        [HttpPost("add")]
        public async Task<IActionResult> ApiAdd([FromBody] AddCartItemDTO body)
        {
            if (CurrentUserId == null) return Unauthorized(new { message = "Ban can dang nhap." });
            if (body == null || body.ProductId <= 0) return BadRequest(new { message = "Du lieu khong hop le." });
            if (body.UserId != CurrentUserId.Value) return Forbid();

            var ok = await _cartService.AddToCartAsync(
                CurrentUserId.Value,
                body.ProductId,
                body.Quantity <= 0 ? 1 : body.Quantity
            );

            return ok
                ? Ok(new { message = " Da them san pham vao gio hang" })
                : StatusCode(500, new { message = "Khong the them san pham." });
        }

        [HttpPut("update/{cartItemId}")]
        public async Task<IActionResult> ApiUpdateQty(int cartItemId, int quantity)
        {
            if (CurrentUserId == null) return Unauthorized(new { message = "Ban can dang nhap." });
            if (quantity < 1) return BadRequest(new { message = "So luong phai >= 1." });

            var ok = await _cartService.UpdateQuantityAsync(cartItemId, quantity);
            return ok ? Ok(new { message = "Cap nhat so luong thanh cong." })
                      : StatusCode(500, new { message = "Loi cap nhat." });
        }

        [HttpDelete("remove/{cartItemId}")]
        public async Task<IActionResult> ApiRemove(int cartItemId)
        {
            if (CurrentUserId == null) return Unauthorized(new { message = "Ban can dang nhap." });

            var ok = await _cartService.RemoveFromCartAsync(cartItemId);
            return ok ? Ok(new { message = "Da xoa san pham." })
                      : StatusCode(500, new { message = "Khong the xoa." });
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> ApiGetCartByUser(int userId)
        {
            if (CurrentUserId == null || CurrentUserId != userId)
            {
                return Forbid();
            }

            var cart = await _cartService.GetCartWithProductsAsync(userId);
            return cart == null ? NotFound() : Ok(cart);
        }

        [HttpGet("count")]
        public async Task<IActionResult> ApiCount()
        {
            if (CurrentUserId == null) return Ok(new { count = 0 });

            var cart = await _cartService.GetCartWithProductsAsync(CurrentUserId.Value);
            int count = cart?.CartItems?.Count ?? 0;

            return Ok(new { count });
        }

        [HttpGet("/Cart/Refresh")]
        public async Task<IActionResult> RefreshCart()
        {
            if (CurrentUserId == null) return PartialView("_CartEmptyPartial");

            var cart = await _cartService.GetCartWithProductsAsync(CurrentUserId.Value);
            if (cart == null || cart.CartItems == null || !cart.CartItems.Any())
                return PartialView("_CartEmptyPartial");

            return PartialView("_CartContentPartial", cart);
        }
    }
}
