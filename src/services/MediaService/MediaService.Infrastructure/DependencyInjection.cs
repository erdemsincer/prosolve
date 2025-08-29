using MediaService.Application.Abstractions;
using MediaService.Infrastructure.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MediaService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddMediaInfrastructure(this IServiceCollection services, IConfiguration cfg)
    {
        var endpoint  = cfg["S3:Endpoint"]   ?? "http://minio:9000";
        var accessKey = cfg["S3:AccessKey"]  ?? "minio";
        var secretKey = cfg["S3:SecretKey"]  ?? "minio123";
        var bucket    = cfg["S3:Bucket"]     ?? "prosolve-media";
        var region    = cfg["S3:Region"]     ?? "us-east-1";

        services.AddSingleton<IMediaStorage>(_ => new S3MediaStorage(endpoint, accessKey, secretKey, bucket, region));
        return services;
    }
}
