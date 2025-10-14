namespace Mangalith.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string FullName { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? LastLoginAtUtc { get; private set; }

    private User()
    {
        Id = Guid.Empty;
        Email = string.Empty;
        PasswordHash = string.Empty;
        FullName = string.Empty;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public User(string email, string passwordHash, string fullName)
    {
        Id = Guid.NewGuid();
        Email = email;
        PasswordHash = passwordHash;
        FullName = fullName;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateLastLogin(DateTime timestampUtc) => LastLoginAtUtc = timestampUtc;

    public void UpdatePassword(string newPasswordHash) => PasswordHash = newPasswordHash;
}
