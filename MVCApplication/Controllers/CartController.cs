using Microsoft.AspNetCore.Mvc;
using MVCApplication.Services.Interfaces;

namespace MVCApplication.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        // Lấy UserId từ Session
        private int? CurrentUserId => HttpContext.Session.GetInt32("UserId");

        // GET: /Cart
        public async Task<IActionResult> Index()
        {
            if (CurrentUserId == null)
                return RedirectToAction("Login", "Accounts");

            var cart = await _cartService.GetCartWithProductsAsync(CurrentUserId.Value);
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> Add(int productId, int quantity)
        {
            if (CurrentUserId == null)
                return RedirectToAction("Login", "Accounts");

            var success = await _cartService.AddToCartAsync(CurrentUserId.Value, productId, quantity);

            TempData[success ? "Success" : "Error"] = success
                ? "Đã thêm sản phẩm vào giỏ!"
                : "Không thể thêm sản phẩm vào giỏ!";

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int cartItemId)
        {
            if (CurrentUserId == null)
                return RedirectToAction("Login", "Accounts");

            var success = await _cartService.RemoveFromCartAsync(cartItemId);

            TempData[success ? "Success" : "Error"] = success
                ? "Xoá sản phẩm thành công!"
                : "Xoá sản phẩm thất bại!";

            return RedirectToAction("Index");
        }

        // API: cập nhật số lượng
        [HttpPut("/api/cart/update/{cartItemId}")]
        public async Task<IActionResult> ApiUpdateQty(int cartItemId, int quantity)
        {
            if (CurrentUserId == null)
                return Unauthorized("Bạn cần đăng nhập");

            if (quantity < 1) return BadRequest("quantity >= 1");

            var ok = await _cartService.UpdateQuantityAsync(cartItemId, quantity);
            return ok ? Ok() : StatusCode(500);
        }

        // API: xoá item
        [HttpDelete("/api/cart/remove/{cartItemId}")]
        public async Task<IActionResult> ApiRemove(int cartItemId)
        {
            if (CurrentUserId == null)
                return Unauthorized("Bạn cần đăng nhập");

            var ok = await _cartService.RemoveFromCartAsync(cartItemId);
            return ok ? Ok() : StatusCode(500);
        }

        [HttpGet("/api/cart/user/{userId}")]
        public async Task<IActionResult> ApiGetCartByUser(int userId)
        {
            var cart = await _cartService.GetCartWithProductsAsync(userId);
            if (cart == null) return NotFound();
            return Ok(cart);
        }

    }
}
