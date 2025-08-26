
using Microsoft.EntityFrameworkCore;
using ProfileService.Infrastructure.Persistence;

namespace ProfileService.Api.Extensions;

public static class MigrationExtensions
{
    public static async Task MigrateAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }
}
