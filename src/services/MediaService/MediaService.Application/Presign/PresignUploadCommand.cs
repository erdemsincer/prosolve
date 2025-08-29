using FluentValidation;
using MediatR;
using MediaService.Application.Abstractions;

namespace MediaService.Application.Presign;

public sealed record PresignUploadCommand(string FileName, string ContentType, string? Folder, int ExpireMinutes = 15) 
    : IRequest<PresignUploadResponse>;

public sealed record PresignUploadResponse(string ObjectKey, string UploadUrl, DateTime ExpiresAtUtc);

public sealed class PresignUploadValidator : AbstractValidator<PresignUploadCommand>
{
    public PresignUploadValidator()
    {
        RuleFor(x => x.FileName).NotEmpty();
        RuleFor(x => x.ContentType).NotEmpty();
        RuleFor(x => x.ExpireMinutes).InclusiveBetween(1, 60);
    }
}

public sealed class PresignUploadHandler(IMediaStorage storage) : IRequestHandler<PresignUploadCommand, PresignUploadResponse>
{
    public Task<PresignUploadResponse> Handle(PresignUploadCommand c, CancellationToken ct)
    {
        var key = storage.GenerateObjectKey(c.Folder, c.FileName);
        var (url, exp) = storage.PresignPut(key, c.ContentType, TimeSpan.FromMinutes(c.ExpireMinutes));
        return Task.FromResult(new PresignUploadResponse(key, url, exp));
    }
}
