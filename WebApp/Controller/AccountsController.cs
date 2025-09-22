using Microsoft.AspNetCore.Mvc;
using WebApp.Models;
using WebApp.Services.Interfaces;

namespace WebApp.Controllers
{
    public class AccountsController : Controller
    {
        private readonly IAccountService _service;
        private readonly IHttpContextAccessor _http;
        public AccountsController(IAccountService service, IHttpContextAccessor http)
        {
            _service = service;
            _http = http;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var user = await _service.LoginAsync(dto);
            if (user == null)
            {
                ViewBag.Error = "Sai tài khoản hoặc mật khẩu";
                return View(dto);
            }

            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Role", user.Role);

            return RedirectToAction("Profile");
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var user = await _service.RegisterAsync(dto);
            if (user == null)
            {
                ViewBag.Error = "Đăng ký thất bại";
                return View(dto);
            }
            return RedirectToAction("Login");
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            var user = await _service.GetByIdAsync(userId.Value);
            if (user == null) return RedirectToAction("Login");

            return View(user);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
