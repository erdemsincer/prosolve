using JobService.Domain.Jobs;
namespace JobService.Application.Jobs;
public sealed record JobDto(Guid Id, Guid OwnerId, string Title, string Description, string City, string District, string[] MediaKeys, JobStatus Status, DateTime CreatedAtUtc, DateTime? PublishedAtUtc);
