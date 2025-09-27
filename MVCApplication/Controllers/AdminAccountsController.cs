using Microsoft.AspNetCore.Mvc;
using MVCApplication.Models;
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

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Accounts");

            var adminId = HttpContext.Session.GetInt32("UserId");
            if (adminId == null) return RedirectToAction("Login", "Accounts");

            var user = await _service.GetByIdAsync(adminId.Value);
            if (user == null) return RedirectToAction("Login", "Accounts");

            var vm = new UpdateProfileViewModel
            {
                UserId = user.UserId,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role,
                IsBanned = user.IsBanned,
                AvatarUrl = user.AvatarUrl
            };

            return View(vm); // Views/AdminAccounts/Profile.cshtml
        }

        // POST: /AdminAccounts/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(UpdateProfileViewModel dto)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Accounts");

            var adminId = HttpContext.Session.GetInt32("UserId");
            if (adminId == null) return RedirectToAction("Login", "Accounts");

            if (!ModelState.IsValid)
            {
                return View("Profile", dto);
            }

            var ok = await _service.UpdateProfileAsync(adminId.Value, dto);
            if (ok == null)
            {
                ViewBag.Error = "Cập nhật thất bại!";
                return View("Profile", dto);
            }

            // reload lại dữ liệu sau khi cập nhật
            var updated = await _service.GetByIdAsync(adminId.Value);
            if (updated == null) return RedirectToAction("Login", "Accounts");

            var vm = new UpdateProfileViewModel
            {
                UserId = updated.UserId,
                Username = updated.Username,
                FullName = updated.FullName,
                Email = updated.Email,
                Phone = updated.Phone,
                Role = updated.Role,
                IsBanned = updated.IsBanned,
                AvatarUrl = updated.AvatarUrl
            };

            TempData["Success"] = "Cập nhật thông tin thành công!";
            return View("Profile", vm);
        }
    }

}
