namespace ComponentsOrderApi.DTOs;


public record RegisterRequest(
    string Email,
    string Password,
    string Name);

public record LoginRequest(
    string Email,
    string Password);

public record ForgotPasswordRequest(
    string Email);

public record ResetPasswordRequest(
    string Token,
    string NewPassword);

public record AuthResponse(
    string Token,
    UserDto User);

public record UserDto(
    int Id,
    string Email,
    string Name,
    string Role);

public record RegisterResponse(
    bool Success,
    bool AlreadyExists,
    string Message);
