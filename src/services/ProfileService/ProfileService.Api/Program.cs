using FluentValidation;
using MassTransit;
using MediatR;
using ProfileService.Api.Consumers;
using ProfileService.Api.Extensions;
using ProfileService.Application.Profiles;
using ProfileService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMediatR(typeof(GetProfileQuery).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(GetProfileQuery).Assembly);
builder.Services.AddProfileInfrastructure(builder.Configuration);

// MassTransit / RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<UserRegisteredConsumer>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        var host = builder.Configuration["Rabbit:Host"] ?? "rabbitmq";
        var user = builder.Configuration["Rabbit:Username"] ?? "guest";
        var pass = builder.Configuration["Rabbit:Password"] ?? "guest";
        cfg.Host(host, "/", h =>
        {
            h.Username(user);
            h.Password(pass);
        });

        // consumer endpoint
        cfg.ReceiveEndpoint("profile.user-registered", e =>
        {
            e.ConfigureConsumer<UserRegisteredConsumer>(ctx);
        });
    });
});

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", () => new { status = "ok", service = "ProfileService" });

// GET /profiles/{userId}
app.MapGet("/profiles/{userId:guid}", async (Guid userId, IMediator mediator, CancellationToken ct) =>
{
    try
    {
        var dto = await mediator.Send(new GetProfileQuery(userId), ct);
        return Results.Ok(dto);
    }
    catch (KeyNotFoundException)
    {
        return Results.NotFound();
    }
});

// PUT /profiles/{userId}
app.MapPut("/profiles/{userId:guid}", async (Guid userId, UpsertProfileCommand body, IMediator mediator, CancellationToken ct) =>
{
    if (userId != body.UserId) return Results.BadRequest(new { message = "UserId mismatch." });
    try
    {
        var dto = await mediator.Send(body, ct);
        return Results.Ok(dto);
    }
    catch (KeyNotFoundException)
    {
        return Results.NotFound();
    }
});
await app.MigrateAsync();
app.Run();