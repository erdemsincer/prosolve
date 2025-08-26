using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProfileService.Application.Abstractions;
using ProfileService.Infrastructure.Persistence;
using ProfileService.Infrastructure.Repositories;

namespace ProfileService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddProfileInfrastructure(this IServiceCollection services, IConfiguration cfg)
    {
        var cs = cfg.GetConnectionString("Default") ?? cfg["ConnectionStrings:Default"]!;
        services.AddDbContext<AppDbContext>(o => o.UseNpgsql(cs, x => x.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));
        services.AddScoped<IProfileRepository, ProfileRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }
}
