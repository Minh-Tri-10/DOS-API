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

        // GET: /Cart
        public async Task<IActionResult> Index()
        {
            int userId = 1; // giả lập login user
            var cart = await _cartService.GetCartWithProductsAsync(userId);
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> Add(int productId, int quantity)
        {
            int userId = 1;
            var success = await _cartService.AddToCartAsync(userId, productId, quantity);

            TempData[success ? "Success" : "Error"] = success
                ? "Đã thêm sản phẩm vào giỏ!"
                : "Không thể thêm sản phẩm vào giỏ!";

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int cartItemId)
        {
            var success = await _cartService.RemoveFromCartAsync(cartItemId);

            TempData[success ? "Success" : "Error"] = success
                ? "Xoá sản phẩm thành công!"
                : "Xoá sản phẩm thất bại!";

            return RedirectToAction("Index");
        }
        // Thêm vào CartController của MVC (khác với action View)
        [HttpPut("/api/cart/update/{cartItemId}")]
        public async Task<IActionResult> ApiUpdateQty(int cartItemId, int quantity)
        {
            if (quantity < 1) return BadRequest("quantity >= 1");
            var ok = await _cartService.UpdateQuantityAsync(cartItemId, quantity);
            return ok ? Ok() : StatusCode(500);
        }

        [HttpDelete("/api/cart/remove/{cartItemId}")]
        public async Task<IActionResult> ApiRemove(int cartItemId)
        {
            var ok = await _cartService.RemoveFromCartAsync(cartItemId);
            return ok ? Ok() : StatusCode(500);
        }

    }
}
