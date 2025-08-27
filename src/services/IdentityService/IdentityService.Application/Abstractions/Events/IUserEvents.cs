using IdentityService.Domain.Users;
namespace IdentityService.Application.Abstractions.Events;
public interface IUserEvents
{
    Task PublishUserRegistered(User user, CancellationToken ct);
}
