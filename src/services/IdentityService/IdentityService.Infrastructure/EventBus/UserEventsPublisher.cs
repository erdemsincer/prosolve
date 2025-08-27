using IdentityService.Application.Abstractions.Events;
using IdentityService.Domain.Users;
using MassTransit;
using ProSolve.Contracts.Identity;

namespace IdentityService.Infrastructure.EventBus;

public sealed class UserEventsPublisher(IPublishEndpoint bus) : IUserEvents
{
    public async Task PublishUserRegistered(User user, CancellationToken ct)
    {
        var msg = new UserRegistered(user.Id, user.Email, user.Role.ToString(), DateTime.UtcNow);
        await bus.Publish(msg, ct);
    }
}
