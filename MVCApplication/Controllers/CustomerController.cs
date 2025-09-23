using Microsoft.AspNetCore.Mvc;

namespace MVCApplication.Controllers
{
    public class CustomerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
