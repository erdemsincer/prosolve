using AiService.Api.Consumers;
using AiService.Application.Abstractions;
using AiService.Application.Stub;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Classifier (stub)
builder.Services.AddSingleton<IJobClassifier, RuleBasedJobClassifier>();

// MassTransit
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<JobCreatedConsumer>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        var host = builder.Configuration["Rabbit:Host"] ?? "rabbitmq";
        var user = builder.Configuration["Rabbit:Username"] ?? "guest";
        var pass = builder.Configuration["Rabbit:Password"] ?? "guest";
        cfg.Host(host, "/", h => { h.Username(user); h.Password(pass); });

        cfg.ReceiveEndpoint("ai.job-created", e =>
        {
            e.ConfigureConsumer<JobCreatedConsumer>(ctx);
        });
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", () => new { status = "ok", service = "AiService" });

app.Run();
