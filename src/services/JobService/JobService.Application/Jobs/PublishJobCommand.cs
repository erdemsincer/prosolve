using MediatR;
using JobService.Application.Abstractions;
using JobService.Domain.Jobs;

namespace JobService.Application.Jobs;

public sealed record PublishJobCommand(Guid JobId) : IRequest<JobDto>;

public sealed class PublishJobHandler(IJobRepository repo, IUnitOfWork uow) : IRequestHandler<PublishJobCommand, JobDto>
{
    public async Task<JobDto> Handle(PublishJobCommand c, CancellationToken ct)
    {
        var job = await repo.GetAsync(c.JobId, ct) ?? throw new KeyNotFoundException("Job not found.");
        job.Publish();
        await repo.UpdateAsync(job, ct);
        await uow.SaveChangesAsync(ct);
        return new JobDto(job.Id, job.OwnerId, job.Title, job.Description, job.Location.City, job.Location.District, job.MediaKeys, job.Status, job.CreatedAtUtc, job.PublishedAtUtc);
    }
}
