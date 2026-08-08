using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ComponentsOrderApi.Data;
using ComponentsOrderApi.Middleware;
using ComponentsOrderApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        }));

var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key not set");
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            NameClaimType = "sub",
            RoleClaimType = "role"
        };

        // Add logging hooks to see why token validation/authentication may fail in production.
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = ctx =>
            {
                var loggerFactory = ctx.HttpContext.RequestServices.GetService<ILoggerFactory>();
                var logger = loggerFactory?.CreateLogger("JwtBearerEvents");
                logger?.LogError(ctx.Exception, "JWT authentication failed");
                return Task.CompletedTask;
            },
            OnTokenValidated = ctx =>
            {
                var loggerFactory = ctx.HttpContext.RequestServices.GetService<ILoggerFactory>();
                var logger = loggerFactory?.CreateLogger("JwtBearerEvents");
                var sub = ctx.Principal?.FindFirst("sub")?.Value ?? ctx.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                logger?.LogInformation("JWT token validated for sub={sub}", sub);

                // Ensure the principal has a ClaimTypes.Role claim so Authorize(Roles=...) works reliably
                var identity = ctx.Principal?.Identity as System.Security.Claims.ClaimsIdentity;
                if (identity != null)
                {
                    var roleClaim = identity.FindFirst("role")?.Value;
                    if (!string.IsNullOrEmpty(roleClaim) && !identity.HasClaim(c => c.Type == System.Security.Claims.ClaimTypes.Role))
                    {
                        identity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, roleClaim));
                    }
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Components Order API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "JWT Bearer token",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() }
    });
});

builder.Services.AddHttpClient<IEmailService, EmailService>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOrderService, OrderService>();

var allowedOrigins = (builder.Configuration["Cors:AllowedOrigins"] ?? "http://localhost:5173;http://localhost:3000")
    .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbSeeder.SeedAsync(db);
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Database initialization failed.");
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
// Run JwtUserMiddleware after UseAuthentication so we don't overwrite the principal created by JwtBearer
app.UseMiddleware<JwtUserMiddleware>();
app.UseAuthorization();
app.MapControllers();

// Health endpoint (simple liveness check)
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
