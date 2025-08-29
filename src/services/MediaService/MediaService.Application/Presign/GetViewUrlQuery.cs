using FluentValidation;
using MediatR;
using MediaService.Application.Abstractions;

namespace MediaService.Application.Presign;

public sealed record GetViewUrlQuery(string ObjectKey, int ExpireMinutes = 15) : IRequest<ViewUrlResponse>;
public sealed record ViewUrlResponse(string Url, DateTime ExpiresAtUtc);

public sealed class GetViewUrlValidator : AbstractValidator<GetViewUrlQuery>
{
    public GetViewUrlValidator() => RuleFor(x => x.ObjectKey).NotEmpty();
}

public sealed class GetViewUrlHandler(IMediaStorage storage) : IRequestHandler<GetViewUrlQuery, ViewUrlResponse>
{
    public Task<ViewUrlResponse> Handle(GetViewUrlQuery q, CancellationToken ct)
    {
        var (url, exp) = storage.PresignGet(q.ObjectKey, TimeSpan.FromMinutes(q.ExpireMinutes));
        return Task.FromResult(new ViewUrlResponse(url, exp));
    }
}
