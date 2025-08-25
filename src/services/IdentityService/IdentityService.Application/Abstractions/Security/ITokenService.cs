namespace IdentityService.Application.Abstractions.Security;

public sealed record JwtPair(string AccessToken, DateTime AccessExpiresAtUtc, string RefreshToken, DateTime RefreshExpiresAtUtc, string TokenType = "Bearer");

public interface ITokenService
{
    JwtPair CreateForUser(Guid userId, string email, string role);
    string NewRefreshToken(); // cryptographically secure random
    TimeSpan AccessTokenLifetime { get; }
    TimeSpan RefreshTokenLifetime { get; }
}
