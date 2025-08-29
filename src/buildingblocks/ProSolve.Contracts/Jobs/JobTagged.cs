namespace ProSolve.Contracts.Jobs;
public sealed record JobTagged(Guid JobId, string[] Tags, int Urgency, DateTime OccurredAtUtc);
