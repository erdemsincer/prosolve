using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using IdentityService.Application.Abstractions.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace IdentityService.Infrastructure.Security;

public sealed class JwtTokenService(IConfiguration cfg) : ITokenService
{
    private readonly string _issuer = cfg["Jwt:Issuer"] ?? "prosolve.identity";
    private readonly string _audience = cfg["Jwt:Audience"] ?? "prosolve.clients";
    private readonly string _key = cfg["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key missing");
    public TimeSpan AccessTokenLifetime => TimeSpan.FromMinutes(int.TryParse(cfg["Jwt:AccessMinutes"], out var m) ? m : 30);
    public TimeSpan RefreshTokenLifetime => TimeSpan.FromDays(int.TryParse(cfg["Jwt:RefreshDays"], out var d) ? d : 14);

    public JwtPair CreateForUser(Guid userId, string email, string role)
    {
        var now = DateTime.UtcNow;
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.Role, role)
        };

        var jwt = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            notBefore: now,
            expires: now.Add(AccessTokenLifetime),
            signingCredentials: creds);

        var access = new JwtSecurityTokenHandler().WriteToken(jwt);
        var refresh = NewRefreshToken();
        return new JwtPair(access, now.Add(AccessTokenLifetime), refresh, now.Add(RefreshTokenLifetime));
    }

    public string NewRefreshToken()
    {
        var bytes = new byte[64];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }
}
