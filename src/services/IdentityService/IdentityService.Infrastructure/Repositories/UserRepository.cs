using Microsoft.EntityFrameworkCore;
using IdentityService.Application.Abstractions;
using IdentityService.Domain.Users;
using IdentityService.Infrastructure.Persistence;

namespace IdentityService.Infrastructure.Repositories;

public sealed class UserRepository(AppDbContext db) : IUserRepository
{
    public async Task AddAsync(User user, CancellationToken ct) => await db.Users.AddAsync(user, ct);
    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct) =>
        await db.Users.FirstOrDefaultAsync(x => x.Email == email.ToLower(), ct);
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct) =>
        await db.Users.FirstOrDefaultAsync(x => x.Id == id, ct);
    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct) =>
        await db.Users.AnyAsync(x => x.Email == email.ToLower(), ct);
    public Task UpdateAsync(User user, CancellationToken ct) { db.Users.Update(user); return Task.CompletedTask; }
}
