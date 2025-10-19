using System.Collections.Generic;
using System.Threading.Tasks;
using AccountAPI.DTOs;
using AccountAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _service;

    public AccountsController(IAccountService service)
    {
        _service = service;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDTO>> Register(RegisterDTO dto)
    {
        var user = await _service.RegisterAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = user.UserId }, user);
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
        var user = await _service.GetByIdAsync(id);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpGet]
    public async Task<IEnumerable<UserDTO>> GetAll() => await _service.GetAllAsync();

    [HttpPut("{id:int}/profile")]
    public async Task<ActionResult<UserDTO>> UpdateProfile(int id, [FromBody] UpdateProfileDTO dto)
    {
        var result = await _service.UpdateProfileAsync(id, dto);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPatch("{id:int}/ban")]
    public async Task<IActionResult> SetBan(int id, [FromBody] BanRequestDTO dto)
    {
        var ok = await _service.SetBanAsync(id, dto.IsBanned);
        return ok ? NoContent() : NotFound();
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO dto)
    {
        var ok = await _service.ChangePasswordAsync(dto);
        return ok ? NoContent() : BadRequest("Wrong current password or user not found");
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO dto)
    {
        var token = await _service.ForgotPasswordAsync(dto);
        return Ok(new { token });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO dto)
    {
        var ok = await _service.ResetPasswordAsync(dto);
        return ok ? NoContent() : BadRequest("Invalid or expired token");
    }
}

