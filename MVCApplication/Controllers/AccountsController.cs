using Microsoft.AspNetCore.Mvc;
using MVCApplication.Models;
using MVCApplication.Services.Interfaces;

namespace MVCApplication.Controllers
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

            // Lưu session
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Role", user.Role);

            // Điều hướng theo Role
            if (user.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("Index", "AdminAccounts");
            else
                return RedirectToAction("Profile", "Accounts");
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
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ViewBag.Error = "Vui lòng nhập email.";
                return View();
            }

            // Luôn trả message chung để tránh lộ email tồn tại
            await _service.ForgotPasswordAsync(email);
            ViewBag.Message = "Nếu email tồn tại, chúng tôi đã gửi hướng dẫn đặt lại mật khẩu.";
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string token, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                ViewBag.Error = "Token không hợp lệ.";
                return View();
            }
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                ViewBag.Error = "Mật khẩu tối thiểu 6 ký tự.";
                ViewBag.Token = token;
                return View();
            }
            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Xác nhận mật khẩu không khớp.";
                ViewBag.Token = token;
                return View();
            }

            var ok = await _service.ResetPasswordAsync(token, newPassword);
            if (!ok)
            {
                ViewBag.Error = "Token không hợp lệ hoặc đã hết hạn.";
                ViewBag.Token = token;
                return View();
            }

            TempData["Message"] = "Đổi mật khẩu thành công. Vui lòng đăng nhập lại.";
            return RedirectToAction("Login");
        }
    }
}
