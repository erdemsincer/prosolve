using FluentValidation;
using MediatR;
using IdentityService.Application.Abstractions;
using IdentityService.Application.Abstractions.Security;

namespace IdentityService.Application.Auth;

public sealed record RefreshTokenCommand(Guid UserId, string RefreshToken) : IRequest<AuthResponse>;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}

public sealed class RefreshTokenCommandHandler(
    IUserRepository users,
    IRefreshTokenRepository refreshTokens,
    ITokenService tokens,
    IUnitOfWork uow
) : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(RefreshTokenCommand cmd, CancellationToken ct)
    {
        var user = await users.GetByIdAsync(cmd.UserId, ct) ?? throw new InvalidOperationException("User not found.");
        var activeRt = await refreshTokens.GetActiveAsync(user.Id, cmd.RefreshToken, ct) ?? throw new InvalidOperationException("Refresh token invalid.");

        // rotate
        var pair = tokens.CreateForUser(user.Id, user.Email, user.Role.ToString());
        await refreshTokens.RevokeAsync(activeRt, pair.RefreshToken, ct);
        await refreshTokens.AddAsync(new Domain.Auth.RefreshToken(user.Id, pair.RefreshToken, pair.RefreshExpiresAtUtc), ct);

        await uow.SaveChangesAsync(ct);
        return new AuthResponse(pair.AccessToken, pair.AccessExpiresAtUtc, pair.RefreshToken, pair.RefreshExpiresAtUtc);
    }
}
