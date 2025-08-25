namespace IdentityService.Application.Auth;

public sealed record AuthResponse(string AccessToken, DateTime AccessExpiresAtUtc, string RefreshToken, DateTime RefreshExpiresAtUtc, string TokenType = "Bearer");
