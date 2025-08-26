using MediatR;
using ProfileService.Application.Abstractions;

namespace ProfileService.Application.Profiles;
public sealed record GetProfileQuery(Guid UserId) : IRequest<ProfileDto>;

public sealed class GetProfileQueryHandler(IProfileRepository repo) : IRequestHandler<GetProfileQuery, ProfileDto>
{
    public async Task<ProfileDto> Handle(GetProfileQuery q, CancellationToken ct)
    {
        var p = await repo.GetAsync(q.UserId, ct) ?? throw new KeyNotFoundException("Profile not found.");
        return new ProfileDto(p.UserId, p.Email, p.Role, p.City, p.District, p.RadiusKm, p.Skills);
    }
}
