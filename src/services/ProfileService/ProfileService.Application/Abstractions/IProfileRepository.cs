using ProfileService.Domain.Profiles;
namespace ProfileService.Application.Abstractions;

public interface IProfileRepository
{
    Task<Profile?> GetAsync(Guid userId, CancellationToken ct);
    Task AddAsync(Profile p, CancellationToken ct);
    Task UpdateAsync(Profile p, CancellationToken ct);
}
