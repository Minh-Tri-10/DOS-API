using Microsoft.AspNetCore.Mvc;
using MVCApplication.Services.Interfaces;
using MVCApplication.Models;

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
            int userId = 1;
            var cart = await _cartService.GetCartByUserIdAsync(userId);

            // Đảm bảo CartItems không null
            cart.CartItems = cart.CartItems ?? new List<CartItem>();

            return View(cart);
        }


        // POST: /Cart/Add
        [HttpPost]
        public async Task<IActionResult> Add(int productId, int quantity)
        {
            int userId = 1;
            var success = await _cartService.AddToCartAsync(userId, productId, quantity);

            if (!success)
                TempData["Error"] = "Không thể thêm sản phẩm vào giỏ!";
            else
                TempData["Success"] = "Đã thêm sản phẩm vào giỏ!";

            return RedirectToAction("Index");
        }

        // POST: /Cart/Remove
        [HttpPost]
        public async Task<IActionResult> Remove(int cartItemId)
        {
            var success = await _cartService.RemoveFromCartAsync(cartItemId);

            if (!success)
                TempData["Error"] = "Xoá sản phẩm thất bại!";
            else
                TempData["Success"] = "Xoá sản phẩm thành công!";

            return RedirectToAction("Index");
        }
    }
}
