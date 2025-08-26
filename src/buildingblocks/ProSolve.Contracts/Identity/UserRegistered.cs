namespace ProSolve.Contracts.Identity;
public sealed record UserRegistered(Guid UserId, string Email, string Role, DateTime OccurredAtUtc);
