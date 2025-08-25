using FluentValidation;
using MediatR;
using IdentityService.Application.Abstractions;
using IdentityService.Application.Abstractions.Security;

namespace IdentityService.Application.Auth;

public sealed record LoginCommand(string Email, string Password) : IRequest<AuthResponse>;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public sealed class LoginCommandHandler(
    IUserRepository users,
    IRefreshTokenRepository refreshTokens,
    IPasswordHasher hasher,
    ITokenService tokens,
    IUnitOfWork uow
) : IRequestHandler<LoginCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(LoginCommand cmd, CancellationToken ct)
    {
        var user = await users.GetByEmailAsync(cmd.Email, ct) ?? throw new InvalidOperationException("Invalid credentials.");
        if (!hasher.Verify(cmd.Password, user.PasswordHash))
            throw new InvalidOperationException("Invalid credentials.");

        var pair = tokens.CreateForUser(user.Id, user.Email, user.Role.ToString());
        // revoke old active tokens optionally:
        await refreshTokens.RevokeAllForUserAsync(user.Id, ct);
        await refreshTokens.AddAsync(new Domain.Auth.RefreshToken(user.Id, pair.RefreshToken, pair.RefreshExpiresAtUtc), ct);

        await uow.SaveChangesAsync(ct);
        return new AuthResponse(pair.AccessToken, pair.AccessExpiresAtUtc, pair.RefreshToken, pair.RefreshExpiresAtUtc);
    }
}
