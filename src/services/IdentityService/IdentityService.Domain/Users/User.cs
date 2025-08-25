

namespace IdentityService.Domain.Users;


public sealed class User
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Email { get; private set; }
    public string? Phone { get; private set; }
    public string PasswordHash { get; private set; }
    public Role Role { get; private set; } = Role.User;
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    private User() { } // EF
    public User(string email, string passwordHash, Role role = Role.User, string? phone = null)
    {
        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        Role = role;
        Phone = phone;
    }

    public void ChangePassword(string newHash)
    {
        PasswordHash = newHash;
        UpdatedAt = DateTime.UtcNow;
    }
}
