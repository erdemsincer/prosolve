using IdentityService.Domain.Auth;

namespace IdentityService.Application.Abstractions;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetActiveAsync(Guid userId, string token, CancellationToken ct);
    Task AddAsync(RefreshToken token, CancellationToken ct);
    Task RevokeAsync(RefreshToken token, string? replacedByToken, CancellationToken ct);
    Task RevokeAllForUserAsync(Guid userId, CancellationToken ct);
}
