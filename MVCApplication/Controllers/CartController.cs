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
    }
}
