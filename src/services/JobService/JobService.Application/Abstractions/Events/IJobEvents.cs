using JobService.Domain.Jobs;
namespace JobService.Application.Abstractions.Events;
public interface IJobEvents
{
    Task PublishJobCreated(Job job, CancellationToken ct);
}
