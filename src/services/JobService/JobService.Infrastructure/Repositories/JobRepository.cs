using Microsoft.EntityFrameworkCore;
using JobService.Application.Abstractions;
using JobService.Domain.Jobs;
using JobService.Infrastructure.Persistence;

namespace JobService.Infrastructure.Repositories;

public sealed class JobRepository(AppDbContext db) : IJobRepository
{
    public async Task AddAsync(Job job, CancellationToken ct) => await db.Jobs.AddAsync(job, ct);
    public async Task<Job?> GetAsync(Guid id, CancellationToken ct) => await db.Jobs.FirstOrDefaultAsync(x => x.Id == id, ct);
    public Task UpdateAsync(Job job, CancellationToken ct) { db.Jobs.Update(job); return Task.CompletedTask; }
    public IQueryable<Job> Query() => db.Jobs.AsNoTracking().AsQueryable();
}
