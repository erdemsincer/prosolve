using MassTransit;
using ProSolve.Contracts.Identity;
using ProfileService.Application.Abstractions;
using ProfileService.Domain.Profiles;

namespace ProfileService.Api.Consumers;

public sealed class UserRegisteredConsumer(IProfileRepository repo, IUnitOfWork uow) : IConsumer<UserRegistered>
{
    public async Task Consume(ConsumeContext<UserRegistered> context)
    {
        var m = context.Message;
        // idempotent: varsa dokunma
        var existing = await repo.GetAsync(m.UserId, context.CancellationToken);
        if (existing is not null) return;

        var p = new Profile(m.UserId, m.Email, m.Role);
        await repo.AddAsync(p, context.CancellationToken);
        await uow.SaveChangesAsync(context.CancellationToken);
    }
}
