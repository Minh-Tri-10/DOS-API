using Microsoft.AspNetCore.Mvc;

namespace MVCApplication.Controllers
{
    public class CartController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
