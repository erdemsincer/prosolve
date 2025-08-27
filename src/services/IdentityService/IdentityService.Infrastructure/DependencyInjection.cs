using IdentityService.Application.Abstractions;
using IdentityService.Application.Abstractions.Events;
using IdentityService.Application.Abstractions.Security;
using IdentityService.Infrastructure.EventBus;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Infrastructure.Repositories;
using IdentityService.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services, IConfiguration cfg)
    {
        var cs = cfg.GetConnectionString("Default") ?? cfg["ConnectionStrings:Default"] ?? "Host=localhost;Port=5432;Database=identity;Username=postgres;Password=postgres";
        services.AddDbContext<AppDbContext>(o =>
        {
            o.UseNpgsql(cs, npg => npg.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddScoped<IUserEvents, UserEventsPublisher>();
        return services;
    }
}
