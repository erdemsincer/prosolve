using Microsoft.EntityFrameworkCore;
using IdentityService.Application.Abstractions;
using IdentityService.Domain.Auth;
using IdentityService.Infrastructure.Persistence;

namespace IdentityService.Infrastructure.Repositories;

public sealed class RefreshTokenRepository(AppDbContext db) : IRefreshTokenRepository
{
    public async Task<RefreshToken?> GetActiveAsync(Guid userId, string token, CancellationToken ct) =>
        await db.RefreshTokens.FirstOrDefaultAsync(x => x.UserId == userId && x.Token == token && x.RevokedAtUtc == null && x.ExpiresAtUtc > DateTime.UtcNow, ct);

    public async Task AddAsync(RefreshToken token, CancellationToken ct) => await db.RefreshTokens.AddAsync(token, ct);

    public Task RevokeAsync(RefreshToken token, string? replacedByToken, CancellationToken ct)
    {
        token.Revoke(replacedByToken);
        db.RefreshTokens.Update(token);
        return Task.CompletedTask;
    }

    public async Task RevokeAllForUserAsync(Guid userId, CancellationToken ct)
    {
        var tokens = await db.RefreshTokens.Where(x => x.UserId == userId && x.RevokedAtUtc == null).ToListAsync(ct);
        foreach (var t in tokens) t.Revoke();
        db.RefreshTokens.UpdateRange(tokens);
    }
}
