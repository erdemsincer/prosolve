using IdentityService.Application.Abstractions.Security;

namespace IdentityService.Infrastructure.Security;
public sealed class BCryptPasswordHasher : IPasswordHasher
{
    public string Hash(string plain) => BCrypt.Net.BCrypt.HashPassword(plain);
    public bool Verify(string plain, string hash) => BCrypt.Net.BCrypt.Verify(plain, hash);
}
