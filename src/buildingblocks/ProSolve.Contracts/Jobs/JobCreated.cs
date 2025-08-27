namespace ProSolve.Contracts.Jobs;
public sealed record JobCreated(Guid JobId, Guid OwnerId, string Title, string City, string District, string[] MediaKeys, DateTime OccurredAtUtc);
