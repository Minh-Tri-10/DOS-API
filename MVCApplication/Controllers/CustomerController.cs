using Microsoft.AspNetCore.Mvc;
using MVCApplication.DTOs;
using MVCApplication.Services.Interfaces;
using System.Threading.Tasks;

namespace MVCApplication.Controllers
{
    public class CustomerController : Controller
    {
        private readonly IProductService _productService;

        public CustomerController(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllAsync();
            return View(products);
        }
    }
}