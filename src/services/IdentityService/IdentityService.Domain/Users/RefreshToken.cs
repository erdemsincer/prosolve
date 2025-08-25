namespace IdentityService.Domain.Auth;

public sealed class RefreshToken
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public string Token { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public DateTime? RevokedAtUtc { get; private set; }
    public string? ReplacedByToken { get; private set; }

    private RefreshToken() { } // EF
    public RefreshToken(Guid userId, string token, DateTime expiresAtUtc)
    {
        UserId = userId;
        Token = token;
        ExpiresAtUtc = expiresAtUtc;
    }
    public bool IsActive => RevokedAtUtc is null && DateTime.UtcNow < ExpiresAtUtc;
    public void Revoke(string? replacedByToken = null)
    {
        RevokedAtUtc = DateTime.UtcNow;
        ReplacedByToken = replacedByToken;
    }
}
