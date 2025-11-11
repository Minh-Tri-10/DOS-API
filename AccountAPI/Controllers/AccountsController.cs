using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AccountAPI.DTOs;
using AccountAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDTO>> Login(LoginDTO dto)
    {
        var auth = await _service.LoginAsync(dto);
        if (auth == null) return Unauthorized("Invalid username or password");
        return Ok(auth);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<UserDTO>> GetById(int id)
    {
        var user = await _service.GetByIdAsync(id);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IEnumerable<UserDTO>> GetAll() => await _service.GetAllAsync();

    [HttpPut("{id:int}/profile")]
    public async Task<ActionResult<UserDTO>> UpdateProfile(
    int id,
    [FromForm] UpdateProfileDTO dto,
    IFormFile? avatarFile)
    {
        var result = await _service.UpdateProfileAsync(id, dto, avatarFile);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPatch("{id:int}/ban")]
    [Authorize(Roles = "Admin")]
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
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO dto)
    {
        var token = await _service.ForgotPasswordAsync(dto);
        return Ok(new { token });
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO dto)
    {
        var ok = await _service.ResetPasswordAsync(dto);
        return ok ? NoContent() : BadRequest("Invalid or expired token");
    }
}
