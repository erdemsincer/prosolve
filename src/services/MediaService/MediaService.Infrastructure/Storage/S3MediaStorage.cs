using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using MediaService.Application.Abstractions;

namespace MediaService.Infrastructure.Storage;

public sealed class S3MediaStorage : IMediaStorage, IDisposable
{
    private readonly string _bucket;
    private readonly IAmazonS3 _s3;

    public S3MediaStorage(string endpoint, string accessKey, string secretKey, string bucket, string region = "us-east-1", bool forcePathStyle = true)
    {
        _bucket = bucket;
        var cfg = new AmazonS3Config
        {
            ServiceURL = endpoint, // "http://minio:9000"
            ForcePathStyle = forcePathStyle
        };
        _s3 = new AmazonS3Client(accessKey, secretKey, cfg);
    }

    public async Task EnsureBucketAsync(CancellationToken ct)
    {
        var buckets = await _s3.ListBucketsAsync(ct);
        if (!buckets.Buckets.Any(b => b.BucketName == _bucket))
        {
            await _s3.PutBucketAsync(new PutBucketRequest { BucketName = _bucket }, ct);
        }
    }

    public string GenerateObjectKey(string? folder, string fileName)
    {
        var ext = Path.GetExtension(fileName);
        var cleanExt = string.IsNullOrWhiteSpace(ext) ? "" : ext.ToLowerInvariant();
        var f = string.IsNullOrWhiteSpace(folder) ? "misc" : folder.Trim().Trim('/');
        var date = DateTime.UtcNow;
        return $"{f}/{date:yyyy/MM}/{Guid.NewGuid():N}{cleanExt}";
    }

    public (string Url, DateTime ExpiresAtUtc) PresignPut(string objectKey, string contentType, TimeSpan expiry)
    {
        var req = new GetPreSignedUrlRequest
        {
            BucketName = _bucket,
            Key = objectKey,
            Verb = HttpVerb.PUT,
            Expires = DateTime.UtcNow.Add(expiry),
            ContentType = contentType
        };
        var url = _s3.GetPreSignedURL(req);
        // Expires tipi DateTime? ise null kontrolü yap
        return (url, req.Expires ?? DateTime.UtcNow.AddMinutes(5));
    }

    public (string Url, DateTime ExpiresAtUtc) PresignGet(string objectKey, TimeSpan expiry)
    {
        var req = new GetPreSignedUrlRequest
        {
            BucketName = _bucket,
            Key = objectKey,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.Add(expiry)
        };
        var url = _s3.GetPreSignedURL(req);
        // Expires tipi DateTime? ise null kontrolü yap
        return (url, req.Expires ?? DateTime.UtcNow.AddMinutes(5));
    }

    public void Dispose() => _s3.Dispose();
}