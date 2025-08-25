using FluentValidation;
using MediatR;
using IdentityService.Application.Abstractions;
using IdentityService.Application.Abstractions.Security;
using IdentityService.Domain.Users;

namespace IdentityService.Application.Auth;

public sealed record RegisterUserCommand(string Email, string Password, string? Phone, Role Role) : IRequest<AuthResponse>;

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        RuleFor(x => x.Role).IsInEnum();
    }
}

public sealed class RegisterUserCommandHandler(
    IUserRepository users,
    IRefreshTokenRepository refreshTokens,
    IPasswordHasher hasher,
    ITokenService tokens,
    IUnitOfWork uow
) : IRequestHandler<RegisterUserCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(RegisterUserCommand cmd, CancellationToken ct)
    {
        if (await users.ExistsByEmailAsync(cmd.Email, ct))
            throw new InvalidOperationException("Email already registered.");

        var hash = hasher.Hash(cmd.Password);
        var user = new User(cmd.Email, hash, cmd.Role, cmd.Phone);
        await users.AddAsync(user, ct);

        // tokens
        var pair = tokens.CreateForUser(user.Id, user.Email, user.Role.ToString());
        var rt = new Domain.Auth.RefreshToken(user.Id, pair.RefreshToken, pair.RefreshExpiresAtUtc);
        await refreshTokens.AddAsync(rt, ct);

        await uow.SaveChangesAsync(ct);

        return new AuthResponse(pair.AccessToken, pair.AccessExpiresAtUtc, pair.RefreshToken, pair.RefreshExpiresAtUtc);
    }
}
