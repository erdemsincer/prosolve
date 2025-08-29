using AiService.Application.Abstractions;
using MassTransit;
using ProSolve.Contracts.Jobs;

namespace AiService.Api.Consumers;

public sealed class JobCreatedConsumer(IJobClassifier classifier, IPublishEndpoint bus) : IConsumer<JobCreated>
{
    public async Task Consume(ConsumeContext<JobCreated> context)
    {
        var m = context.Message;
        // Basit sınıflandırma: title içinde; description yoksa sadece title ile çalışır (MVP)
        var (tags, urgency) = classifier.Classify(m.Title, "", m.City, m.District, m.MediaKeys);
        await bus.Publish(new JobTagged(m.JobId, tags, urgency, DateTime.UtcNow), context.CancellationToken);
    }
}
