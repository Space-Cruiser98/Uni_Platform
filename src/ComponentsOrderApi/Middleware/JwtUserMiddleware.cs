using System.Security.Claims;
using ComponentsOrderApi.Services;

namespace ComponentsOrderApi.Middleware;

public class JwtUserMiddleware
{
    private readonly RequestDelegate _next;

    public JwtUserMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IAuthService authService)
    {
        var bearer = context.Request.Headers.Authorization.ToString();
        var user = authService.GetUserFromToken(bearer);
        if (user != null)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("sub", user.Id.ToString()),
                new Claim("role", user.Role)
            };
            var identity = new ClaimsIdentity(claims, "Bearer");
            context.User = new ClaimsPrincipal(identity);
        }
        await _next(context);
    }
}
