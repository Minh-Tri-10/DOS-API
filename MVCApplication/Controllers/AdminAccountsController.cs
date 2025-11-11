using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MVCApplication.Models;
using MVCApplication.Services.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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
                if (IsAjaxRequest())
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Không thể cập nhật trạng thái tài khoản."
                    });
                }

                TempData["Error"] = "Khong the cap nhat trang thai tai khoan.";
                return RedirectToAction(nameof(Index));
            }

            if (IsAjaxRequest())
            {
                return Json(new { success = true, isBanned });
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
        public async Task<IActionResult> UpdateProfile(UpdateProfileViewModel dto, IFormFile? avatarFile)
        {
            if (!TryGetAuthenticatedUserId(out var adminId))
            {
                await SignOutAsync();
                return RedirectToAction("Login", "Accounts");
            }

            dto.UserId = adminId; // ✅ gửi ID để API biết người đang sửa

            // ✅ Kiểm tra model trước khi gọi API
            if (!ModelState.IsValid)
            {
                await PrepareHeaderAndKeepEditing(dto, adminId);
                return View("Profile", dto);
            }

            try
            {
                var updatedUser = await _service.UpdateProfileAsync(adminId, dto, avatarFile);
                if (updatedUser == null)
                {
                    ViewBag.Error = "Cập nhật thất bại!";
                    await PrepareHeaderAndKeepEditing(dto, adminId);
                    return View("Profile", dto);
                }

                // ✅ Làm mới claims (avatar, tên, email mới)
                await RefreshClaimsAsync(updatedUser);

                // ✅ Hiển thị thông báo thành công
                ViewBag.Success = "Cập nhật thông tin thành công!";
                return View("Profile", new UpdateProfileViewModel
                {
                    UserId = updatedUser.UserId,
                    Username = updatedUser.Username,
                    FullName = updatedUser.FullName,
                    Email = updatedUser.Email,
                    Phone = updatedUser.Phone,
                    Role = updatedUser.Role,
                    IsBanned = updatedUser.IsBanned,
                    AvatarUrl = updatedUser.AvatarUrl
                });
            }
            catch (ValidationException ex)
            {
                Console.WriteLine("⚠️ ValidationException: " + ex.Message);

                // ✅ Nếu service truyền raw JSON lỗi qua ex.Data["ApiErrors"]
                if (ex.Data["ApiErrors"] is string rawJson)
                {
                    try
                    {
                        var root = System.Text.Json.JsonDocument.Parse(rawJson).RootElement;
                        if (root.TryGetProperty("errors", out var errorsProp))
                        {
                            foreach (var kv in errorsProp.EnumerateObject())
                            {
                                foreach (var msg in kv.Value.EnumerateArray())
                                {
                                    ModelState.AddModelError(kv.Name, msg.GetString() ?? "Dữ liệu không hợp lệ.");
                                }
                            }
                        }
                    }
                    catch
                    {
                        ModelState.AddModelError(string.Empty, "Dữ liệu không hợp lệ.");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }

                // ✅ Giữ lại form + header khi có lỗi
                await PrepareHeaderAndKeepEditing(dto, adminId);
                return View("Profile", dto);
            }
            catch (HttpRequestException)
            {
                ViewBag.Error = "Không thể kết nối tới máy chủ. Vui lòng thử lại.";
                await PrepareHeaderAndKeepEditing(dto, adminId);
                return View("Profile", dto);
            }
        }
        private async Task PrepareHeaderAndKeepEditing(UpdateProfileViewModel dto, int userId)
        {
            var user = await _service.GetByIdAsync(userId);
            if (user != null)
            {
                // Giữ phần header trên cùng
                ViewBag.Header = new
                {
                    user.FullName,
                    user.Email,
                    user.Role,
                    user.AvatarUrl
                };

                // Giữ các field bị disable
                dto.Username = user.Username;
                dto.Role = user.Role;
                dto.IsBanned = user.IsBanned;
            }

            ViewBag.KeepEditing = true;
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                ViewBag.IsAjax = true;
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

        private bool IsAjaxRequest()
        {
            return string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
