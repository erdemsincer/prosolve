using JobService.Application.Abstractions.Events;
using JobService.Domain.Jobs;
using MassTransit;
using ProSolve.Contracts.Jobs;

namespace JobService.Infrastructure.EventBus;

public sealed class JobEventsPublisher(IPublishEndpoint bus) : IJobEvents
{
    public async Task PublishJobCreated(Job job, CancellationToken ct)
    {
        var m = new JobCreated(job.Id, job.OwnerId, job.Title, job.Location.City, job.Location.District, job.MediaKeys, DateTime.UtcNow);
        await bus.Publish(m, ct);
    }
}
