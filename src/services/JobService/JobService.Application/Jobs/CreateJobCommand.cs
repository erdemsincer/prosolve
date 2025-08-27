// Application/Jobs/CreateJobCommand.cs
using System.Text.Json.Serialization;
using FluentValidation;
using MediatR;
using JobService.Application.Abstractions;
using JobService.Application.Abstractions.Events;
using JobService.Domain.Jobs;

namespace JobService.Application.Jobs;

public sealed record CreateJobCommand(
    string Title,
    string Description,
    string City,
    string District,
    string[] MediaKeys
) : IRequest<JobDto>
{
    // Body'den gelmez; endpoint token'dan set eder.
    [JsonIgnore] 
    public Guid OwnerId { get; init; }
}

public sealed class CreateJobValidator : AbstractValidator<CreateJobCommand>
{
    public CreateJobValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.City).NotEmpty();
        RuleFor(x => x.District).NotEmpty();
        // OwnerId artık body'de değil; burada doğrulamayacağız.
    }
}

public sealed class CreateJobHandler(IJobRepository repo, IUnitOfWork uow, IJobEvents events)
    : IRequestHandler<CreateJobCommand, JobDto>
{
    public async Task<JobDto> Handle(CreateJobCommand c, CancellationToken ct)
    {
        var job = new Job(
            c.OwnerId,
            c.Title,
            c.Description,
            new Location(c.City, c.District),
            c.MediaKeys
        );

        await repo.AddAsync(job, ct);
        await uow.SaveChangesAsync(ct);

        await events.PublishJobCreated(job, ct); // AI/Media için event

        return new JobDto(
            job.Id, job.OwnerId, job.Title, job.Description,
            job.Location.City, job.Location.District, job.MediaKeys,
            job.Status, job.CreatedAtUtc, job.PublishedAtUtc
        );
    }
}
