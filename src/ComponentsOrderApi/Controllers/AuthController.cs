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
public async Task<IActionResult> Register(
    [FromBody] RegisterRequest request,
    CancellationToken ct)
{
    if (string.IsNullOrWhiteSpace(request.Email) ||
        string.IsNullOrWhiteSpace(request.Password) ||
        string.IsNullOrWhiteSpace(request.Name))
    {
        return BadRequest(
            "Email, Password and Name are required.");
    }

    var result =
        await _auth.RegisterAsync(request, ct);

    if (result.AlreadyExists)
    {
        return BadRequest(result.Message);
    }

    return Ok(result);
}

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct)
    {
        try
        {
            var result =
                await _auth.LoginAsync(request, ct);

            if (result == null)
            {
                return Unauthorized(
                    "Invalid email or password.");
            }

            return Ok(result);
        }
        catch (EmailNotConfirmedException)
        {
            return StatusCode(
                403,
                new
                {
                    message =
                        "Please verify your email address before signing in."
                });
        }
    }

    [HttpGet("verify-email")]
    public async Task<IActionResult> VerifyEmail(
        [FromQuery] string token,
        CancellationToken ct)
    {
        var success =
            await _auth.VerifyEmailAsync(token, ct);

        if (!success)
        {
            return BadRequest(
                "Invalid or expired verification link.");
        }

        return Ok(new
        {
            message =
                "Email verified successfully."
        });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken ct)
    {
        await _auth.RequestPasswordResetAsync(
            request.Email,
            ct);

        // Always return the same response.
        // This prevents email enumeration.
        return Ok(new
        {
            message =
                "If an account with that email exists, a password reset link has been sent."
        });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken ct)
    {
        var success =
            await _auth.ResetPasswordAsync(
                request,
                ct);

        if (!success)
        {
            return BadRequest(
                "Invalid or expired password reset link.");
        }

        return Ok(new
        {
            message =
                "Password reset successfully."
        });
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var user =
            _auth.GetUserFromClaims(User);

        if (user == null)
            return Unauthorized();

        return Ok(user);
    }
}