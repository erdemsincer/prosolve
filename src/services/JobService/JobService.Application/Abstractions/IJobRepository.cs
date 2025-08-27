using JobService.Domain.Jobs;
namespace JobService.Application.Abstractions;

public interface IJobRepository
{
    Task AddAsync(Job job, CancellationToken ct);
    Task<Job?> GetAsync(Guid id, CancellationToken ct);
    Task UpdateAsync(Job job, CancellationToken ct);
    IQueryable<Job> Query(); // filtre için
}
