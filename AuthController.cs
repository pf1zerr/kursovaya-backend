using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using StudyHub.API.DTOs;
using StudyHub.API.Services;

namespace StudyHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _auth;
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public AuthController(AuthService auth) => _auth = auth;

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var result = await _auth.RegisterAsync(dto);
        if (result == null) return BadRequest(new { message = "Email already in use" });
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var result = await _auth.LoginAsync(dto);
        if (result == null) return Unauthorized(new { message = "Invalid credentials" });
        return Ok(result);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var profile = await _auth.GetProfileAsync(UserId);
        return profile == null ? NotFound() : Ok(profile);
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
    {
        var (success, message) = await _auth.ChangePasswordAsync(UserId, dto);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }
}
