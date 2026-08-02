using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ComponentsOrderApi.DTOs;
using ComponentsOrderApi.Services;

namespace ComponentsOrderApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Email, Password and Name are required.");

        var result = await _auth.RegisterAsync(request, ct);
        if (result == null)
            return BadRequest("Email already registered.");

        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await _auth.LoginAsync(request, ct);
        if (result == null)
            return Unauthorized("Invalid email or password.");

        return Ok(result);
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var user = _auth.GetUserFromClaims(User);
        if (user == null) return Unauthorized();
        return Ok(user);
    }
}
