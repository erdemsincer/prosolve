using Microsoft.EntityFrameworkCore;
using IdentityService.Domain.Users;
using IdentityService.Domain.Auth;

namespace IdentityService.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Migration işlemleri için parametresiz ctor
    public AppDbContext() { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Migration ve tasarım zamanı için bağlantı dizesi
            optionsBuilder.UseNpgsql("Host=pg-identity;Port=5432;Database=identity;Username=postgres;Password=postgres");
        }
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.HasPostgresExtension("uuid-ossp");

        b.Entity<User>(e =>
        {
            e.ToTable("users");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Email).HasColumnName("email").IsRequired();
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Phone).HasColumnName("phone");
            e.Property(x => x.PasswordHash).HasColumnName("password_hash").IsRequired();
            e.Property(x => x.Role).HasColumnName("role").HasConversion<int>();
            e.Property(x => x.IsActive).HasColumnName("is_active");
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        });

        b.Entity<RefreshToken>(e =>
        {
            e.ToTable("refresh_tokens");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
            e.HasIndex(x => new { x.UserId, x.Token }).IsUnique();
            e.Property(x => x.Token).HasColumnName("token").IsRequired();
            e.Property(x => x.ExpiresAtUtc).HasColumnName("expires_at_utc").IsRequired();
            e.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc").HasDefaultValueSql("now()");
            e.Property(x => x.RevokedAtUtc).HasColumnName("revoked_at_utc");
            e.Property(x => x.ReplacedByToken).HasColumnName("replaced_by_token");
        });
    }
}