using Microsoft.AspNetCore.Mvc;
using MVCApplication.DTOs;                // <-- dùng DTO ở thư mục DTOs
using MVCApplication.Services.Interfaces;

namespace MVCApplication.Controllers
{
    // Nhóm các API Ajax của giỏ hàng cho gọn
    [Route("api/cart")]
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        public CartController(ICartService cartService) => _cartService = cartService;

        private int? CurrentUserId => HttpContext.Session.GetInt32("UserId");

        // Trang giỏ hàng (MVC view)
        [HttpGet("/Cart")] // hoặc [Route("/Cart")]
        public async Task<IActionResult> Index()
        {
            if (CurrentUserId == null) return RedirectToAction("Login", "Accounts");
            var cart = await _cartService.GetCartWithProductsAsync(CurrentUserId.Value);
            return View(cart);
        }

        // ---------- AJAX APIs ----------

        // POST api/cart/add
        [HttpPost("add")]
        public async Task<IActionResult> ApiAdd([FromBody] AddCartItemDTO body)
        {
            if (CurrentUserId == null) return Unauthorized(new { message = "Bạn cần đăng nhập" });
            if (body == null || body.ProductId <= 0) return BadRequest(new { message = "Dữ liệu không hợp lệ" });

            var ok = await _cartService.AddToCartAsync(
                CurrentUserId.Value,
                body.ProductId,
                body.Quantity <= 0 ? 1 : body.Quantity
            );

            return ok
                ? Ok(new { message = "Đã thêm/cộng dồn sản phẩm vào giỏ!" })
                : StatusCode(500, new { message = "Không thể thêm sản phẩm" });
        }

        // PUT api/cart/update/{cartItemId}?quantity=3
        [HttpPut("update/{cartItemId}")]
        public async Task<IActionResult> ApiUpdateQty(int cartItemId, int quantity)
        {
            if (CurrentUserId == null) return Unauthorized(new { message = "Bạn cần đăng nhập" });
            if (quantity < 1) return BadRequest(new { message = "Số lượng phải >= 1" });

            var ok = await _cartService.UpdateQuantityAsync(cartItemId, quantity);
            return ok ? Ok(new { message = "Cập nhật số lượng thành công" })
                      : StatusCode(500, new { message = "Lỗi cập nhật" });
        }

        // DELETE api/cart/remove/{cartItemId}
        [HttpDelete("remove/{cartItemId}")]
        public async Task<IActionResult> ApiRemove(int cartItemId)
        {
            if (CurrentUserId == null) return Unauthorized(new { message = "Bạn cần đăng nhập" });

            var ok = await _cartService.RemoveFromCartAsync(cartItemId);
            return ok ? Ok(new { message = "Đã xóa sản phẩm" })
                      : StatusCode(500, new { message = "Không thể xóa" });
        }

        // GET api/cart/user/{userId}  (endpoint debug/preview)
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> ApiGetCartByUser(int userId)
        {
            var cart = await _cartService.GetCartWithProductsAsync(userId);
            return cart == null ? NotFound() : Ok(cart);
        }
    }
}
