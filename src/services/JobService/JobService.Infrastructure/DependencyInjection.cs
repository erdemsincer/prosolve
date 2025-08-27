 using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using JobService.Application.Abstractions;
using JobService.Application.Abstractions.Events;
using JobService.Infrastructure.EventBus;
using JobService.Infrastructure.Persistence;
using JobService.Infrastructure.Repositories;

namespace JobService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddJobInfrastructure(this IServiceCollection services, IConfiguration cfg)
    {
        var cs = cfg.GetConnectionString("Default") ?? cfg["ConnectionStrings:Default"]!;
        services.AddDbContext<AppDbContext>(o => o.UseNpgsql(cs, x => x.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));
        services.AddScoped<IJobRepository, JobRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IJobEvents, JobEventsPublisher>();
        return services;
    }
}
