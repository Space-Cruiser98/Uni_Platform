using ComponentsOrderApi.DTOs;

namespace ComponentsOrderApi.Services;

public interface IAuthService
{
    Task<AuthResponse?> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default);
    UserDto? GetUserFromClaims(System.Security.Claims.ClaimsPrincipal principal);
    UserDto? GetUserFromToken(string? bearerToken);
}
