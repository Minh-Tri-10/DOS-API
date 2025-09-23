// Controllers/AccountsController.cs
using Microsoft.AspNetCore.Mvc;
using AccountAPI.DTOs;
using AccountAPI.Services.Interfaces;
using AccountAPI.DTOs.AccountAPI.DTOs;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _service;
    public AccountsController(IAccountService service) => _service = service;

    [HttpPost("register")]
    public async Task<ActionResult<UserDTO>> Register(RegisterDTO dto)
    {
        var user = await _service.RegisterAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = user.UserId }, user);  // <- UserId
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDTO>> Login(LoginDTO dto)
    {
        var user = await _service.LoginAsync(dto);
        if (user == null) return Unauthorized("Invalid username or password");
        return Ok(user);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserDTO>> GetById(int id)
    {
        var u = await _service.GetByIdAsync(id);
        return u is null ? NotFound() : Ok(u);
    }

    [HttpGet]
    public async Task<IEnumerable<UserDTO>> GetAll() => await _service.GetAllAsync();
    [HttpPut("{id:int}")]
    public async Task<ActionResult<UserDTO>> UpdateProfile(int id, [FromBody] UpdateProfileDTO dto)
    {
        var result = await _service.UpdateProfileAsync(id, dto);
        return result is null ? NotFound() : Ok(result);
    }

    // PATCH: api/accounts/{id}/ban
    // Controllers/AccountsController.cs (API)
    [HttpPatch("{id:int}/ban")]
    public async Task<IActionResult> SetBan(int id, [FromBody] BanRequestDTO dto)
    {
        Console.WriteLine($"[API] SetBan -> id={id}, isBanned={dto.IsBanned}");
        var ok = await _service.SetBanAsync(id, dto.IsBanned);
        return ok ? NoContent() : NotFound();
    }

    // POST: api/accounts/change-password
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO dto)
    {
        var ok = await _service.ChangePasswordAsync(dto);
        return ok ? NoContent() : BadRequest("Wrong current password or user not found");
    }

    // POST: api/accounts/forgot-password  (trả token test)
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO dto)
    {
        var token = await _service.ForgotPasswordAsync(dto);
        // Để test nhanh: trả token (prod thì gửi email)
        return Ok(new { message = "If the email exists, a reset token was created.", token });
    }

    // POST: api/accounts/reset-password
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO dto)
    {
        var ok = await _service.ResetPasswordAsync(dto);
        return ok ? NoContent() : BadRequest("Invalid or expired token");
    }
}
