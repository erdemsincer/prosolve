namespace MediaService.Application.Abstractions;

public interface IMediaStorage
{
    Task EnsureBucketAsync(CancellationToken ct);
    string GenerateObjectKey(string? folder, string fileName);
    (string Url, DateTime ExpiresAtUtc) PresignPut(string objectKey, string contentType, TimeSpan expiry);
    (string Url, DateTime ExpiresAtUtc) PresignGet(string objectKey, TimeSpan expiry);
}
