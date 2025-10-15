namespace Mangalith.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string FullName { get; private set; }
    public string? Username { get; private set; }
    public string? Avatar { get; private set; }
    public string? Bio { get; private set; }
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public DateTime? LastLoginAtUtc { get; private set; }

    // Propiedades de navegación
    public ICollection<Manga> CreatedMangas { get; private set; } = new List<Manga>();
    public ICollection<Chapter> CreatedChapters { get; private set; } = new List<Chapter>();
    public ICollection<MangaFile> UploadedFiles { get; private set; } = new List<MangaFile>();

    private User()
    {
        Id = Guid.Empty;
        Email = string.Empty;
        PasswordHash = string.Empty;
        FullName = string.Empty;
        Role = UserRole.Reader;
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public User(string email, string passwordHash, string fullName, string? username = null)
    {
        Id = Guid.NewGuid();
        Email = email;
        PasswordHash = passwordHash;
        FullName = fullName;
        Username = username ?? email.Split('@')[0]; // Nombre de usuario por defecto desde email
        Role = UserRole.Reader;
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateLastLogin(DateTime timestampUtc) 
    {
        LastLoginAtUtc = timestampUtc;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdatePassword(string newPasswordHash) 
    {
        PasswordHash = newPasswordHash;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateProfile(string fullName, string? username, string? bio)
    {
        FullName = fullName;
        Username = username;
        Bio = bio;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateAvatar(string avatarPath)
    {
        Avatar = avatarPath;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateRole(UserRole role)
    {
        Role = role;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}

public enum UserRole
{
    Reader = 0,
    Uploader = 1,
    Moderator = 2,
    Administrator = 3
}
