using Microsoft.EntityFrameworkCore;
using ProfileService.Application.Abstractions;
using ProfileService.Domain.Profiles;
using ProfileService.Infrastructure.Persistence;

namespace ProfileService.Infrastructure.Repositories;

public sealed class ProfileRepository(AppDbContext db) : IProfileRepository
{
    public Task<Profile?> GetAsync(Guid userId, CancellationToken ct) =>
        db.Profiles.FirstOrDefaultAsync(x => x.UserId == userId, ct)!;

    public async Task AddAsync(Profile p, CancellationToken ct) => await db.Profiles.AddAsync(p, ct);

    public Task UpdateAsync(Profile p, CancellationToken ct) { db.Profiles.Update(p); return Task.CompletedTask; }
}

public sealed class UnitOfWork(AppDbContext db) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
