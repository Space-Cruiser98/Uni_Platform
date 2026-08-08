using ComponentsOrderApi.DTOs;
using System.Security.Claims;

namespace ComponentsOrderApi.Services;

public interface IAuthService
{
Task<RegisterResponse> RegisterAsync(
    RegisterRequest request,
    CancellationToken ct = default);

    Task<AuthResponse?> LoginAsync(
        LoginRequest request,
        CancellationToken ct = default);

    Task<bool> VerifyEmailAsync(
        string token,
        CancellationToken ct = default);

    Task RequestPasswordResetAsync(
        string email,
        CancellationToken ct = default);

    Task<bool> ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken ct = default);

    UserDto? GetUserFromClaims(
        ClaimsPrincipal principal);

    UserDto? GetUserFromToken(
        string? bearerToken);
}