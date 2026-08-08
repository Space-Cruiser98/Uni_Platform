using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using ComponentsOrderApi.Data;
using ComponentsOrderApi.DTOs;
using ComponentsOrderApi.Entities;

namespace ComponentsOrderApi.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private readonly IEmailService _emailService;

    public AuthService(
        AppDbContext db,
        IConfiguration config,
        IEmailService emailService)
    {
        _db = db;
        _config = config;
        _emailService = emailService;
    }

public async Task<RegisterResponse> RegisterAsync(
    RegisterRequest request,
    CancellationToken ct = default)
{
    var email = request.Email.Trim().ToLowerInvariant();

    if (await _db.Users.AnyAsync(u => u.Email == email, ct))
    {
        return new RegisterResponse(
            false,
            true,
            "Email already registered.");
    }

    var verificationToken = GenerateSecureToken();

    var user = new User
    {
        Email = email,
        PasswordHash =
            BCrypt.Net.BCrypt.HashPassword(request.Password),
        Name = request.Name.Trim(),
        Role = "Student",
        CreatedAt = DateTime.UtcNow,

        EmailConfirmed = false,

        EmailVerificationToken = verificationToken,
        EmailVerificationTokenExpiresAt =
            DateTime.UtcNow.AddHours(24)
    };

    _db.Users.Add(user);

    await _db.SaveChangesAsync(ct);

    var frontendUrl =
        _config["Frontend:Url"]
        ?? "http://localhost:5173";

    var verificationUrl =
        $"{frontendUrl.TrimEnd('/')}/verify-email?token={Uri.EscapeDataString(verificationToken)}";

    await _emailService.SendEmailVerificationAsync(
        user.Email,
        user.Name,
        verificationUrl,
        ct);

    return new RegisterResponse(
        true,
        false,
        "Registration successful. Please check your email to verify your account.");
}
    public async Task<AuthResponse?> LoginAsync(
        LoginRequest request,
        CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == email, ct);

        if (user == null ||
            !BCrypt.Net.BCrypt.Verify(
                request.Password,
                user.PasswordHash))
        {
            return null;
        }

        if (!user.EmailConfirmed)
        {
            throw new EmailNotConfirmedException();
        }

        var token = GenerateJwt(user);

        return new AuthResponse(
            token,
            ToUserDto(user));
    }

    public async Task<bool> VerifyEmailAsync(
        string token,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        var user = await _db.Users
            .FirstOrDefaultAsync(
                u => u.EmailVerificationToken == token,
                ct);

        if (user == null)
            return false;

        if (!user.EmailVerificationTokenExpiresAt.HasValue ||
            user.EmailVerificationTokenExpiresAt.Value < DateTime.UtcNow)
        {
            return false;
        }

        user.EmailConfirmed = true;

        // Token can only be used once.
        user.EmailVerificationToken = null;
        user.EmailVerificationTokenExpiresAt = null;

        await _db.SaveChangesAsync(ct);

        return true;
    }

    public async Task RequestPasswordResetAsync(
        string email,
        CancellationToken ct = default)
    {
        email = email.Trim().ToLowerInvariant();

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == email, ct);

        // Do not reveal whether an email exists.
        if (user == null)
            return;

        var token = GenerateSecureToken();

        user.PasswordResetToken = token;

        user.PasswordResetTokenExpiresAt =
            DateTime.UtcNow.AddMinutes(30);

        await _db.SaveChangesAsync(ct);

        var frontendUrl =
            _config["Frontend:Url"]
            ?? "http://localhost:5173";

        var resetUrl =
            $"{frontendUrl.TrimEnd('/')}/reset-password?token={Uri.EscapeDataString(token)}";

        await _emailService.SendPasswordResetAsync(
            user.Email,
            user.Name,
            resetUrl,
            ct);
    }

    public async Task<bool> ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Token) ||
            string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return false;
        }

        var user = await _db.Users
            .FirstOrDefaultAsync(
                u => u.PasswordResetToken == request.Token,
                ct);

        if (user == null)
            return false;

        if (!user.PasswordResetTokenExpiresAt.HasValue ||
            user.PasswordResetTokenExpiresAt.Value < DateTime.UtcNow)
        {
            return false;
        }

        user.PasswordHash =
            BCrypt.Net.BCrypt.HashPassword(
                request.NewPassword);

        // One-time token.
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiresAt = null;

        await _db.SaveChangesAsync(ct);

        return true;
    }

    public UserDto? GetUserFromClaims(
        ClaimsPrincipal principal)
    {
        var idClaim =
            principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? principal.FindFirst("sub")?.Value;

        var email =
            principal.FindFirst(ClaimTypes.Email)?.Value
            ?? principal.FindFirst("email")?.Value;

        var name =
            principal.FindFirst(ClaimTypes.Name)?.Value
            ?? principal.FindFirst("name")?.Value;

        var role =
            principal.FindFirst(ClaimTypes.Role)?.Value
            ?? principal.FindFirst("role")?.Value;

        if (string.IsNullOrEmpty(idClaim) ||
            !int.TryParse(idClaim, out var id))
        {
            return null;
        }

        return new UserDto(
            id,
            email ?? "",
            name ?? "",
            role ?? "Student");
    }

    public UserDto? GetUserFromToken(
        string? bearerToken)
    {
        if (string.IsNullOrWhiteSpace(bearerToken) ||
            !bearerToken.StartsWith(
                "Bearer ",
                StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var token =
            bearerToken["Bearer ".Length..].Trim();

        if (string.IsNullOrEmpty(token))
            return null;

        var principal = ValidateToken(token);

        if (principal == null)
            return null;

        return GetUserFromClaims(principal);
    }

    private ClaimsPrincipal? ValidateToken(
        string token)
    {
        var jwtKey = _config["Jwt:Key"];

        if (string.IsNullOrWhiteSpace(jwtKey))
            return null;

        var handler = new JwtSecurityTokenHandler();

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey));

        try
        {
            var principal = handler.ValidateToken(
                token,
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,

                    ValidateIssuer = true,
                    ValidIssuer = _config["Jwt:Issuer"],

                    ValidateAudience = true,
                    ValidAudience = _config["Jwt:Audience"],

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,

                    NameClaimType = "sub",
                    RoleClaimType = "role"
                },
                out var validatedToken);

            if (validatedToken is not JwtSecurityToken jwt ||
                !jwt.Header.Alg.Equals(
                    SecurityAlgorithms.HmacSha256,
                    StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }

    private string GenerateJwt(User user)
    {
        var jwtKey =
            _config["Jwt:Key"]
            ?? throw new InvalidOperationException(
                "Jwt:Key not set");

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey));

        var creds = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var expires =
            DateTime.UtcNow.AddMinutes(60);

        var claims = new[]
        {
            new Claim("sub", user.Id.ToString()),
            new Claim("email", user.Email),
            new Claim("name", user.Name),
            new Claim("role", user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expires,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }

    private static string GenerateSecureToken()
    {
        return Convert.ToHexString(
            RandomNumberGenerator.GetBytes(32));
    }

    private static UserDto ToUserDto(User user)
    {
        return new UserDto(
            user.Id,
            user.Email,
            user.Name,
            user.Role);
    }
}

public class EmailNotConfirmedException : Exception
{
    public EmailNotConfirmedException()
        : base("Please verify your email address before signing in.")
    {
    }
}