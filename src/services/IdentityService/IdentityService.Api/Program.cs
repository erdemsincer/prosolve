using System.Security.Claims;
using FluentValidation;
using IdentityService.Api.Extensions;
using IdentityService.Application.Auth;
using IdentityService.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Swagger + JWT desteği
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// MediatR + FluentValidation
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(RegisterUserCommand).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(RegisterUserCommand).Assembly);

// Infra (DbContext, repos, jwt service, hasher)
builder.Services.AddIdentityInfrastructure(builder.Configuration);

// JWT Bearer
var key = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new()
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
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

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "IdentityService" }));

// Endpoints: Register
app.MapPost("/auth/register", async (RegisterUserCommand cmd, IValidator<RegisterUserCommand> v, IMediator mediator, CancellationToken ct) =>
{
    var vr = await v.ValidateAsync(cmd, ct);
    if (!vr.IsValid) return Results.ValidationProblem(vr.ToDictionary());
    try
    {
        var res = await mediator.Send(cmd, ct);
        return Results.Ok(res);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

// Login
app.MapPost("/auth/login", async (LoginCommand cmd, IValidator<LoginCommand> v, IMediator mediator, CancellationToken ct) =>
{
    var vr = await v.ValidateAsync(cmd, ct);
    if (!vr.IsValid) return Results.ValidationProblem(vr.ToDictionary());
    try
    {
        var res = await mediator.Send(cmd, ct);
        return Results.Ok(res);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

// Refresh
app.MapPost("/auth/refresh", async (RefreshTokenCommand cmd, IValidator<RefreshTokenCommand> v, IMediator mediator, CancellationToken ct) =>
{
    var vr = await v.ValidateAsync(cmd, ct);
    if (!vr.IsValid) return Results.ValidationProblem(vr.ToDictionary());
    try
    {
        var res = await mediator.Send(cmd, ct);
        return Results.Ok(res);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

// Me (authorized)
app.MapGet("/auth/me", (ClaimsPrincipal user) =>
{
    var sub = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirst("sub")?.Value;
    var email = user.FindFirstValue(ClaimTypes.Email);
    var role = user.FindFirstValue(ClaimTypes.Role);
    return Results.Ok(new { userId = sub, email, role });
}).RequireAuthorization();

await app.MigrateAsync();
app.Run();