using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVCApplication.Services.Interfaces;

namespace MVCApplication.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        private readonly IProductService _productService;

        public CustomerController(IProductService productService)
        {
            _productService = productService;
        }

        private int? CurrentUserId
        {
            get
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                return int.TryParse(userIdClaim, out var id) ? id : null;
            }
        }

        public async Task<IActionResult> Index()
        {
            if (CurrentUserId == null) return RedirectToAction("Login", "Accounts");

            var products = await _productService.GetAllAsync();
            return View(products);
        }
    }
}
