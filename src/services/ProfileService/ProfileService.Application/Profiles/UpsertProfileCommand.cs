using FluentValidation;
using MediatR;
using ProfileService.Application.Abstractions;

namespace ProfileService.Application.Profiles;

public sealed record UpsertProfileCommand(Guid UserId, string City, string District, int RadiusKm, string[] Skills) : IRequest<ProfileDto>;

public sealed class UpsertProfileValidator : AbstractValidator<UpsertProfileCommand>
{
    public UpsertProfileValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.RadiusKm).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpsertProfileHandler(IProfileRepository repo, IUnitOfWork uow) : IRequestHandler<UpsertProfileCommand, ProfileDto>
{
    public async Task<ProfileDto> Handle(UpsertProfileCommand c, CancellationToken ct)
    {
        var p = await repo.GetAsync(c.UserId, ct) ?? throw new KeyNotFoundException("Profile not found.");
        p.Update(c.City, c.District, c.RadiusKm, c.Skills);
        await repo.UpdateAsync(p, ct);
        await uow.SaveChangesAsync(ct);
        return new ProfileDto(p.UserId, p.Email, p.Role, p.City, p.District, p.RadiusKm, p.Skills);
    }
}
