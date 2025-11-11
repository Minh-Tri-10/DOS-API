using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AccountAPI.DTOs;
using AccountAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// API chịu trách nhiệm cho mọi thao tác liên quan đến tài khoản người dùng.
// Bộ lọc [Authorize] mặc định khóa toàn bộ action, các action anonymous phải ghi chú rõ.
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _service;

    public AccountsController(IAccountService service)
    {
        _service = service;
    }

    // Đăng ký tài khoản mới; cho phép truy cập không cần JWT vì người dùng chưa có token.
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<UserDTO>> Register(RegisterDTO dto)
    {
        try
        {
            var user = await _service.RegisterAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = user.UserId }, user);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // Đăng nhập -> nhận AuthResponse (token + thông tin user) nếu thành công.
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDTO>> Login(LoginDTO dto)
    {
        var auth = await _service.LoginAsync(dto);
        if (auth == null) return Unauthorized("Invalid username or password");
        return Ok(auth);
    }

    // Được client MVC dùng để hiển thị hồ sơ ngay cả trước khi xác thực (ví dụ màn reset password).
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<UserDTO>> GetById(int id)
    {
        var user = await _service.GetByIdAsync(id);
        return user is null ? NotFound() : Ok(user);
    }

    // Danh sách người dùng chỉ dành cho Admin.
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IEnumerable<UserDTO>> GetAll() => await _service.GetAllAsync();

    // Cập nhật thông tin hồ sơ và/hoặc ảnh đại diện; body là form-data.
    [HttpPut("{id:int}/profile")]
    public async Task<ActionResult<UserDTO>> UpdateProfile(
    int id,
    [FromForm] UpdateProfileDTO dto,
    IFormFile? avatarFile)
    {
        var result = await _service.UpdateProfileAsync(id, dto, avatarFile);
        return result is null ? NotFound() : Ok(result);
    }

    // Điều chỉnh trạng thái ban; khi bị ban user không thể đăng nhập (kiểm tra tại AccountService.LoginAsync).
    [HttpPatch("{id:int}/ban")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SetBan(int id, [FromBody] BanRequestDTO dto)
    {
        var ok = await _service.SetBanAsync(id, dto.IsBanned);
        return ok ? NoContent() : NotFound();
    }

    // Yêu cầu JWT: người dùng tự đổi mật khẩu bằng cách gửi current password.
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO dto)
    {
        var ok = await _service.ChangePasswordAsync(dto);
        return ok ? NoContent() : BadRequest("Wrong current password or user not found");
    }

    // Khởi tạo token reset password và gửi qua email; mở cho anonymous để phục hồi tài khoản.
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO dto)
    {
        var token = await _service.ForgotPasswordAsync(dto);
        return Ok(new { token });
    }

    // Dùng token reset để cập nhật mật khẩu mới.
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO dto)
    {
        var ok = await _service.ResetPasswordAsync(dto);
        return ok ? NoContent() : BadRequest("Invalid or expired token");
    }
}
