namespace ComponentsOrderApi.DTOs;

public record RegisterRequest(string Email, string Password, string Name);
public record LoginRequest(string Email, string Password);

public record AuthResponse(string Token, UserDto User);

public record UserDto(int Id, string Email, string Name, string Role);
