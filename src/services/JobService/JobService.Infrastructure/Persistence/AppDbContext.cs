using Microsoft.EntityFrameworkCore;
using JobService.Domain.Jobs;

namespace JobService.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Job> Jobs => Set<Job>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.HasPostgresExtension("uuid-ossp");

        b.Entity<Job>(e =>
        {
            e.ToTable("jobs");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.OwnerId).HasColumnName("owner_id").IsRequired();
            e.Property(x => x.Title).HasColumnName("title").IsRequired().HasMaxLength(120);
            e.Property(x => x.Description).HasColumnName("description").IsRequired().HasMaxLength(2000);
            e.OwnsOne(x => x.Location, loc =>
            {
                loc.Property(p => p.City).HasColumnName("city").IsRequired();
                loc.Property(p => p.District).HasColumnName("district").IsRequired();
                loc.Property(p => p.Lat).HasColumnName("lat");
                loc.Property(p => p.Lng).HasColumnName("lng");
                loc.HasIndex(p => new { p.City, p.District }); // index owned type üzerinden eklenmeli
            });
            e.Property(x => x.MediaKeys).HasColumnName("media_keys").HasColumnType("text[]");
            e.Property(x => x.RequestedSkills).HasColumnName("requested_skills").HasColumnType("text[]");
            e.Property(x => x.Urgency).HasColumnName("urgency");
            e.Property(x => x.Status).HasColumnName("status").HasConversion<int>();
            e.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc").HasDefaultValueSql("now()");
            e.Property(x => x.PublishedAtUtc).HasColumnName("published_at_utc");
            e.HasIndex(x => new { x.Status, x.OwnerId });
            // e.HasIndex("city", "district"); // kaldırıldı, owned type index yukarıda
        });
    }
}