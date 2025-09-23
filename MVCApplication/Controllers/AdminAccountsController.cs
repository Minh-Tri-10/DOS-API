using Microsoft.AspNetCore.Mvc;
using MVCApplication.Services.Interfaces;

namespace MVCApplication.Controllers
{
    public class AdminAccountsController : Controller
    {
        private readonly IAccountService _service;
        public AdminAccountsController(IAccountService service) => _service = service;

        // Danh sách tài khoản
        public async Task<IActionResult> Index()
        {
            // Kiểm tra role trước khi load danh sách
            if (!IsAdmin()) return RedirectToAction("Login", "Accounts");

            var list = await _service.GetAllAsync();
            return View(list);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleBan(int id, bool isBanned)
        {
            Console.WriteLine($"[FE] ToggleBan -> id={id}, isBanned={isBanned}"); // phải là True khi bấm Ban
            if (!IsAdmin()) return RedirectToAction("Login", "Accounts");

            var ok = await _service.SetBanAsync(id, isBanned);
            if (!ok) TempData["Error"] = "Không thể cập nhật trạng thái tài khoản";
            return RedirectToAction("Index");
        }

        private bool IsAdmin()
            => HttpContext.Session.GetString("Role")?.Equals("Admin", StringComparison.OrdinalIgnoreCase) ?? false;
    }

}
