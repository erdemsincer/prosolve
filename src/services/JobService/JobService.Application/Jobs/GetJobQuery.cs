using MediatR;
using JobService.Application.Abstractions;

namespace JobService.Application.Jobs;

public sealed record GetJobQuery(Guid JobId) : IRequest<JobDto>;

public sealed class GetJobHandler(IJobRepository repo) : IRequestHandler<GetJobQuery, JobDto>
{
    public async Task<JobDto> Handle(GetJobQuery q, CancellationToken ct)
    {
        var j = await repo.GetAsync(q.JobId, ct) ?? throw new KeyNotFoundException("Job not found.");
        return new JobDto(j.Id, j.OwnerId, j.Title, j.Description, j.Location.City, j.Location.District, j.MediaKeys, j.Status, j.CreatedAtUtc, j.PublishedAtUtc);
    }
}
