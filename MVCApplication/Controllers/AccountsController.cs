using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MVCApplication.Models;
using MVCApplication.Services.Interfaces;
using System.Globalization;

namespace MVCApplication.Controllers
{
    public class AccountsController : Controller
    {
        private readonly IAccountService _service;

        public AccountsController(IAccountService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated ?? false)
            {
                return RedirectToAction("Index", User.IsInRole("Admin") ? "AdminAccounts" : "Customer");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var auth = await _service.LoginAsync(dto);
            if (auth == null)
            {
                ViewBag.Error = "Sai tai khoan hoac mat khau.";
                return View(dto);
            }

            JwtSecurityToken jwt;
            try
            {
                jwt = new JwtSecurityTokenHandler().ReadJwtToken(auth.AccessToken);
            }
            catch
            {
                ViewBag.Error = "Token dang nhap khong hop le.";
                return View(dto);
            }

            var claims = new List<Claim>(jwt.Claims)
            {
                new Claim("access_token", auth.AccessToken),
                new Claim("avatar_url", auth.User.AvatarUrl ?? string.Empty),
                new Claim("full_name", auth.User.FullName ?? string.Empty),
                new Claim("username", auth.User.Username)
            };

            if (!string.IsNullOrWhiteSpace(auth.User.Email))
            {
                claims.Add(new Claim(ClaimTypes.Email, auth.User.Email));
            }
            claims.RemoveAll(c => c.Type == ClaimTypes.Role);
            var normalizedRole = string.IsNullOrWhiteSpace(auth.User.Role)
                ? "User"
                : CultureInfo.InvariantCulture.TextInfo.ToTitleCase(auth.User.Role.Trim().ToLowerInvariant());
            claims.Add(new Claim(ClaimTypes.Role, normalizedRole));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = false,
                    ExpiresUtc = auth.ExpiresAtUtc
                });

            return auth.User.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                ? RedirectToAction("Index", "AdminAccounts")
                : RedirectToAction("Index", "Customer");
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
                ViewBag.Error = "Dang ky that bai.";
                return View(dto);
            }
            return RedirectToAction("Login");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var userId))
            {
                await SignOutAsync();
                return RedirectToAction("Login");
            }

            var user = await _service.GetByIdAsync(userId);
            if (user == null)
            {
                await SignOutAsync();
                return RedirectToAction("Login");
            }

            var vm = new UpdateProfileViewModel
            {
                UserId = user.UserId,
                Username = user.Username,
                Role = user.Role,
                IsBanned = user.IsBanned,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                AvatarUrl = user.AvatarUrl
            };

            return View(vm);
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await SignOutAsync();
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ViewBag.Error = "Vui long nhap email.";
                return View();
            }

            await _service.ForgotPasswordAsync(email);
            ViewBag.Message = "Neu email ton tai, chung toi da gui huong dan dat lai mat khau.";
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
                ViewBag.Error = "Token khong hop le.";
                return View();
            }
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                ViewBag.Error = "Mat khau toi thieu 6 ky tu.";
                ViewBag.Token = token;
                return View();
            }
            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Xac nhan mat khau khong khop.";
                ViewBag.Token = token;
                return View();
            }

            var ok = await _service.ResetPasswordAsync(token, newPassword);
            if (!ok)
            {
                ViewBag.Error = "Token khong hop le hoac da het han.";
                ViewBag.Token = token;
                return View();
            }

            TempData["Message"] = "Doi mat khau thanh cong. Vui long dang nhap lai.";
            return RedirectToAction("Login");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(UpdateProfileViewModel dto, IFormFile? avatarFile)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var userId))
            {
                await SignOutAsync();
                return RedirectToAction("Login");
            }

            if (!ModelState.IsValid)
            {
                var user = await _service.GetByIdAsync(userId);
                if (user != null)
                {
                    dto.UserId = user.UserId;
                    dto.Username = user.Username;
                    dto.Role = user.Role;
                    dto.IsBanned = user.IsBanned;
                    dto.AvatarUrl ??= user.AvatarUrl;
                }
                ViewBag.KeepEditing = true;
                return View("Profile", dto);
            }

            if (avatarFile != null && avatarFile.Length > 0)
            {
                var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsDir))
                    Directory.CreateDirectory(uploadsDir);

                var fileName = "{Guid.NewGuid()}{Path.GetExtension(avatarFile.FileName)}";
                var savePath = Path.Combine(uploadsDir, fileName);

                using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    await avatarFile.CopyToAsync(stream);
                }

                dto.AvatarUrl = "/uploads/{fileName}";
            }

            var updatedUser = await _service.UpdateProfileAsync(userId, dto);
            if (updatedUser == null)
            {
                ViewBag.Error = "Cap nhat that bai.";
                ViewBag.KeepEditing = true;
                return View("Profile", dto);
            }

            await RefreshAuthenticatedUserClaimsAsync(updatedUser);

            var vm = new UpdateProfileViewModel
            {
                UserId = updatedUser.UserId,
                Username = updatedUser.Username,
                Role = updatedUser.Role,
                IsBanned = updatedUser.IsBanned,
                FullName = updatedUser.FullName,
                Email = updatedUser.Email,
                Phone = updatedUser.Phone,
                AvatarUrl = updatedUser.AvatarUrl
            };

            ViewBag.KeepEditing = false;
            ViewBag.Success = "Cap nhat thong tin thanh cong!";
            return View("Profile", vm);
        }

        private async Task RefreshAuthenticatedUserClaimsAsync(UserViewModel updatedUser)
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
