namespace JobService.Domain.Jobs;

public sealed class Job
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid OwnerId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public Location Location { get; private set; } = null!;
    public string[] MediaKeys { get; private set; } = Array.Empty<string>();
    public string[] RequestedSkills { get; private set; } = Array.Empty<string>(); // AI/sonra manuel
    public int Urgency { get; private set; } = 0; // 0-100
    public JobStatus Status { get; private set; } = JobStatus.Draft;
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public DateTime? PublishedAtUtc { get; private set; }

    private Job() { } // EF
    public Job(Guid ownerId, string title, string description, Location loc, IEnumerable<string> mediaKeys)
    {
        OwnerId = ownerId; Title = title; Description = description; Location = loc;
        MediaKeys = mediaKeys?.ToArray() ?? Array.Empty<string>();
    }

    public void Publish()
    {
        if (Status != JobStatus.Draft) throw new InvalidOperationException("Only draft can be published.");
        Status = JobStatus.Published;
        PublishedAtUtc = DateTime.UtcNow;
    }
}
