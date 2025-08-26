using Microsoft.EntityFrameworkCore;
using ProfileService.Domain.Profiles;

namespace ProfileService.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public AppDbContext() { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql("Host=pg-profile;Port=5432;Database=profile;Username=postgres;Password=postgres");
        }
    }

    public DbSet<Profile> Profiles => Set<Profile>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.HasPostgresExtension("uuid-ossp");
        b.Entity<Profile>(e =>
        {
            e.ToTable("profiles");
            e.HasKey(x => x.UserId);
            e.Property(x => x.UserId).HasColumnName("user_id");
            e.Property(x => x.Email).HasColumnName("email").IsRequired();
            e.Property(x => x.Role).HasColumnName("role").IsRequired();
            e.Property(x => x.City).HasColumnName("city");
            e.Property(x => x.District).HasColumnName("district");
            e.Property(x => x.RadiusKm).HasColumnName("radius_km");
            e.Property(x => x.Skills).HasColumnName("skills").HasColumnType("text[]");
            e.Property(x => x.RatingAvg).HasColumnName("rating_avg");
            e.Property(x => x.RatingCount).HasColumnName("rating_count");
            e.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc").HasDefaultValueSql("now()");
            e.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc");
        });
    }
}