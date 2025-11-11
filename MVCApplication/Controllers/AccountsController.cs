using System;
using System.Net.Http;
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
    // Controller giao tiep AccountAPI thong qua IAccountService.
    public class AccountsController : Controller
    {
        private readonly IAccountService _service;

        public AccountsController(IAccountService service)
        {
            _service = service;
        }

        [HttpGet]
        // Hien thi form dang nhap neu nguoi dung chua dang nhap.
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated ?? false)
            {
                return RedirectToAction("Index", User.IsInRole("Admin") ? "AdminAccounts" : "Customer");
            }
            return View();
        }

        [HttpPost]
        // Xu ly form dang nhap, goi API va luu JWT vao cookie neu thanh cong.
        public async Task<IActionResult> Login(LoginViewModel dto)
        {
            if (!ModelState.IsValid) return View(dto);

            AuthResponseViewModel? auth;
            try
            {
                auth = await _service.LoginAsync(dto);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, "Khong the ket noi may chu. Vui long thu lai.");
                return View(dto);
            }

            JwtSecurityToken jwt;
            try
            {
                jwt = new JwtSecurityTokenHandler().ReadJwtToken(auth!.AccessToken);
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Token dang nhap khong hop le.");
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
        // Render form dang ky nguoi dung moi.
        public IActionResult Register() => View();

        [HttpPost]
        // Validate va goi API AccountAPI de tao tai khoan moi.
        public async Task<IActionResult> Register(RegisterViewModel dto)
        {
            if (!ModelState.IsValid) return View(dto);

            try
            {
                var user = await _service.RegisterAsync(dto);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Dang ky that bai.");
                    return View(dto);
                }

                TempData["RegisterSuccess"] = "Dang ky thanh cong! Vui long dang nhap.";
                return RedirectToAction("Login");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, "Khong the ket noi toi may chu. Vui long thu lai.");
                return View(dto);
            }
        }

        [Authorize]
        [HttpGet]
        // Tai thong tin chi tiet nguoi dung tu API de hien thi trang Profile.
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
        // Huy cookie hien tai va quay ve trang dang nhap.
        public async Task<IActionResult> Logout()
        {
            await SignOutAsync();
            return RedirectToAction("Login");
        }

        [HttpGet]
        // Render form de nguoi dung yeu cau token reset mat khau.
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
        // Hien thi form nhap mat khau moi bang token reset.
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
        // Cap nhat ho so thong qua AccountAPI, dong bo lai claims sau khi thanh cong.
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

            // Goi formdata toi API va Upload len Cloudinary tai API
            var updatedUser = await _service.UpdateProfileAsync(userId, dto, avatarFile);

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
            ViewBag.Success = "Cap nhat thanh cong!";
            return View("Profile", vm);
        }


        // Sau khi user cap nhat profile, cap nhat lai cookie claims cho dong bo UI.
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

        // Dong bo viec sign-out cookie (su dung khi token/claims khong hop le).
        private async Task SignOutAsync()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}
