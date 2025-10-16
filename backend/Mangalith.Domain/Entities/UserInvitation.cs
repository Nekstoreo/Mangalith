using System.Security.Cryptography;
using System.Text;

namespace Mangalith.Domain.Entities;

public class UserInvitation
{
    public Guid Id { get; private set; }
    public string Email { get; private set; }
    public UserRole TargetRole { get; private set; }
    public Guid InvitedByUserId { get; private set; }
    public User InvitedBy { get; private set; } = null!;
    public string Token { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? AcceptedAtUtc { get; private set; }
    public Guid? AcceptedByUserId { get; private set; }
    public User? AcceptedBy { get; private set; }

    // Propiedades computadas
    public bool IsExpired => DateTime.UtcNow > ExpiresAtUtc;
    public bool IsAccepted => AcceptedAtUtc.HasValue;
    public bool IsValid => !IsExpired && !IsAccepted;

    private UserInvitation()
    {
        Id = Guid.NewGuid();
        Email = string.Empty;
        Token = string.Empty;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public UserInvitation(
        string email,
        UserRole targetRole,
        Guid invitedByUserId,
        TimeSpan? expirationPeriod = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty", nameof(email));
        
        if (invitedByUserId == Guid.Empty)
            throw new ArgumentException("InvitedByUserId cannot be empty", nameof(invitedByUserId));

        Id = Guid.NewGuid();
        Email = email.ToLowerInvariant();
        TargetRole = targetRole;
        InvitedByUserId = invitedByUserId;
        Token = GenerateSecureToken();
        CreatedAtUtc = DateTime.UtcNow;
        ExpiresAtUtc = DateTime.UtcNow.Add(expirationPeriod ?? TimeSpan.FromDays(7)); // 7 días por defecto
    }

    public bool AcceptInvitation(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty", nameof(userId));

        if (!IsValid)
            return false;

        AcceptedAtUtc = DateTime.UtcNow;
        AcceptedByUserId = userId;
        return true;
    }

    public void ExtendExpiration(TimeSpan additionalTime)
    {
        if (IsAccepted)
            throw new InvalidOperationException("Cannot extend expiration of an accepted invitation");

        ExpiresAtUtc = ExpiresAtUtc.Add(additionalTime);
    }

    public void ExpireInvitation()
    {
        if (IsAccepted)
            throw new InvalidOperationException("Cannot expire an accepted invitation");

        ExpiresAtUtc = DateTime.UtcNow.AddMinutes(-1); // Establecer como expirada
    }

    private static string GenerateSecureToken()
    {
        // Generar un token seguro de 32 bytes (256 bits)
        using var rng = RandomNumberGenerator.Create();
        var tokenBytes = new byte[32];
        rng.GetBytes(tokenBytes);
        
        // Convertir a string base64 URL-safe
        return Convert.ToBase64String(tokenBytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }
}