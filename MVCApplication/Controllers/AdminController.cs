using Microsoft.AspNetCore.Mvc;

namespace MVCApplication.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
