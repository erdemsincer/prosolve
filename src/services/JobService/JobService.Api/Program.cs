// File: src/services/JobService/JobService.Api/Program.cs
using System.Security.Claims;
using System.Text;
using FluentValidation;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc; // [FromServices]
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using JobService.Application.Jobs;
using JobService.Infrastructure;
using JobService.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Swagger (Bearer)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });
    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// MediatR & FluentValidation & Infra
builder.Services.AddMediatR(typeof(CreateJobCommand).Assembly);
builder.Services.AddValidatorsFromAssemblyContaining<CreateJobValidator>(); // net tarama
// İstersen garanti: builder.Services.AddScoped<IValidator<CreateJobCommand>, CreateJobValidator>();
builder.Services.AddJobInfrastructure(builder.Configuration);

// MassTransit (RabbitMQ)
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((ctx, cfg) =>
    {
        var host = builder.Configuration["Rabbit:Host"] ?? "rabbitmq";
        var user = builder.Configuration["Rabbit:Username"] ?? "guest";
        var pass = builder.Configuration["Rabbit:Password"] ?? "guest";
        cfg.Host(host, "/", h => { h.Username(user); h.Password(pass); });
    });
});

// === JWT Authentication ===
var jwtIssuer   = builder.Configuration["Jwt:Issuer"]   ?? "prosolve.identity";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "prosolve.clients";
var jwtKey      = builder.Configuration["Jwt:Key"]      ?? "DEV_SUPER_SECRET_CHANGE_ME";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => new { status = "ok", service = "JobService" });

// ===== Jobs =====

// Create (OwnerId token'dan alınır) — korumalı endpoint
app.MapPost("/jobs", async (
    CreateJobCommand cmd,
    [FromServices] IValidator<CreateJobCommand> v, // DI'dan çözülür
    IMediator mediator,
    HttpContext http,
    CancellationToken ct) =>
{
    var vr = await v.ValidateAsync(cmd, ct);
    if (!vr.IsValid) return Results.ValidationProblem(vr.ToDictionary());

    // Token'dan OwnerId (öncelik NameIdentifier, yoksa sub)
    var ownerIdStr = http.User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? http.User.FindFirst("sub")?.Value;

    if (!Guid.TryParse(ownerIdStr, out var ownerId))
        return Results.Unauthorized();

    var cmdWithOwner = cmd with { OwnerId = ownerId };
    var dto = await mediator.Send(cmdWithOwner, ct);
    return Results.Ok(dto);
})
.RequireAuthorization(); // JWT zorunlu

// Publish — korumalı
app.MapPost("/jobs/{id:guid}/publish", async (Guid id, IMediator mediator, CancellationToken ct) =>
{
    try
    {
        var dto = await mediator.Send(new PublishJobCommand(id), ct);
        return Results.Ok(dto);
    }
    catch (KeyNotFoundException) { return Results.NotFound(); }
    catch (InvalidOperationException ex) { return Results.BadRequest(new { message = ex.Message }); }
})
.RequireAuthorization();

// Get by id — public (istersen RequireAuthorization ekle)
app.MapGet("/jobs/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
{
    try { return Results.Ok(await mediator.Send(new GetJobQuery(id), ct)); }
    catch (KeyNotFoundException) { return Results.NotFound(); }
});

// Basit liste — public
app.MapGet("/jobs", (string? city, string? district, int? status, JobService.Application.Abstractions.IJobRepository repo) =>
{
    var q = repo.Query();
    if (!string.IsNullOrWhiteSpace(city)) q = q.Where(j => j.Location.City == city);
    if (!string.IsNullOrWhiteSpace(district)) q = q.Where(j => j.Location.District == district);
    if (status is not null) q = q.Where(j => (int)j.Status == status.Value);

    var data = q.OrderByDescending(j => j.CreatedAtUtc)
        .Take(100)
        .Select(j => new JobDto(
            j.Id,
            j.OwnerId,
            j.Title,
            j.Description,
            j.Location.City,
            j.Location.District,
            j.MediaKeys,
            j.Status,
            j.CreatedAtUtc,
            j.PublishedAtUtc))
        .ToList();

    return Results.Ok(data);
});

await app.MigrateAsync();
app.Run();
