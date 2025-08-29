using FluentValidation;
using MediatR;
using MediaService.Application.Presign;
using MediaService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMediatR(typeof(PresignUploadCommand).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(PresignUploadCommand).Assembly);
builder.Services.AddMediaInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", () => new { status = "ok", service = "MediaService" });

// POST /media/presign  -> { fileName, contentType, folder?, expireMinutes? }
app.MapPost("/media/presign", async (PresignUploadCommand cmd, IValidator<PresignUploadCommand> v, IMediator mediator, CancellationToken ct) =>
{
    var vr = await v.ValidateAsync(cmd, ct);
    if (!vr.IsValid) return Results.ValidationProblem(vr.ToDictionary());
    var res = await mediator.Send(cmd, ct);
    return Results.Ok(res);
});

// GET /media/view-url?key=...&expireMinutes=15
app.MapGet("/media/view-url", async (string key, int expireMinutes, IMediator mediator, CancellationToken ct) =>
{
    var res = await mediator.Send(new GetViewUrlQuery(key, expireMinutes <= 0 ? 15 : expireMinutes), ct);
    return Results.Ok(res);
});

app.Run();
