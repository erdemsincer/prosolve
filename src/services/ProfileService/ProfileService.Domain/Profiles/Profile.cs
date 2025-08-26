namespace ProfileService.Domain.Profiles;

public sealed class Profile
{
    public Guid UserId { get; private set; }   // PK = UserId
    public string Email { get; private set; } = string.Empty;
    public string Role { get; private set; } = "User"; // "User" | "Pro" | "Admin"
    public string City { get; private set; } = "";
    public string District { get; private set; } = "";
    public int RadiusKm { get; private set; } = 10;
    public string[] Skills { get; private set; } = Array.Empty<string>();
    public double RatingAvg { get; private set; }
    public int RatingCount { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; private set; }

    private Profile() { } // EF

    public Profile(Guid userId, string email, string role)
    {
        UserId = userId;
        Email = email;
        Role = role;
    }

    public void Update(string city, string district, int radiusKm, string[] skills)
    {
        City = city?.Trim() ?? "";
        District = district?.Trim() ?? "";
        RadiusKm = Math.Max(0, radiusKm);
        Skills = skills ?? Array.Empty<string>();
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
