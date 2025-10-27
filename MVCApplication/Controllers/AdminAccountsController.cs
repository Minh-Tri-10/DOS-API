using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MVCApplication.Models;
using MVCApplication.Services.Interfaces;

namespace MVCApplication.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminAccountsController : Controller
    {
        private readonly IAccountService _service;
        private readonly ILogger<AdminAccountsController> _logger;

        public AdminAccountsController(IAccountService service, ILogger<AdminAccountsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
            _logger.LogInformation("AdminAccounts Index accessed by user {User} with role claim {Role}. Authenticated={Authenticated}",
                User.Identity?.Name ?? "(unknown)",
                roleClaim ?? "(none)",
                User.Identity?.IsAuthenticated ?? false);

            var list = await _service.GetAllAsync();
            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleBan(int id, bool isBanned)
        {
            var ok = await _service.SetBanAsync(id, isBanned);
            if (!ok)
            {
                TempData["Error"] = "Khong the cap nhat trang thai tai khoan.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            if (!TryGetAuthenticatedUserId(out var adminId))
            {
                return RedirectToAction("Login", "Accounts");
            }

            var user = await _service.GetByIdAsync(adminId);
            if (user == null)
            {
                await SignOutAsync();
                return RedirectToAction("Login", "Accounts");
            }

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

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(UpdateProfileViewModel dto)
        {
            if (!TryGetAuthenticatedUserId(out var adminId))
            {
                return RedirectToAction("Login", "Accounts");
            }

            if (!ModelState.IsValid)
            {
                return View("Profile", dto);
            }

            var updated = await _service.UpdateProfileAsync(adminId, dto);
            if (updated == null)
            {
                ViewBag.Error = "Cap nhat that bai.";
                return View("Profile", dto);
            }

            await RefreshClaimsAsync(updated);

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

            TempData["Success"] = "Cap nhat thong tin thanh cong!";
            return View("Profile", vm);
        }

        private bool TryGetAuthenticatedUserId(out int userId)
        {
            userId = 0;
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdClaim, out userId);
        }

        private async Task RefreshClaimsAsync(UserViewModel updatedUser)
        {
            var claims = User.Claims
                .Where(c => c.Type != "avatar_url" && c.Type != "full_name" && c.Type != ClaimTypes.Email && c.Type != "username")
                .ToList();

            claims.Add(new Claim("avatar_url", updatedUser.AvatarUrl ?? string.Empty));
            claims.Add(new Claim("full_name", updatedUser.FullName ?? string.Empty));
            claims.Add(new Claim("username", updatedUser.Username));

            if (!string.IsNullOrWhiteSpace(updatedUser.Email))
            {
                claims.Add(new Claim(ClaimTypes.Email, updatedUser.Email));
            }

            var accessToken = claims.FirstOrDefault(c => c.Type == "access_token")?.Value;
            DateTimeOffset? expiresUtc = null;
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                try
                {
                    var jwt = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
                    expiresUtc = jwt.ValidTo;
                }
                catch
                {
                    expiresUtc = DateTimeOffset.UtcNow.AddMinutes(30);
                }
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties
                {
                    IsPersistent = false,
                    ExpiresUtc = expiresUtc ?? DateTimeOffset.UtcNow.AddMinutes(30)
                });
        }

        private async Task SignOutAsync()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}
