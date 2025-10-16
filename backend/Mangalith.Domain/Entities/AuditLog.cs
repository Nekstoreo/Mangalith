namespace Mangalith.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public string Action { get; private set; }
    public string Resource { get; private set; }
    public string? ResourceId { get; private set; }
    public string? Details { get; private set; }
    public string IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public bool Success { get; private set; }
    public DateTime TimestampUtc { get; private set; }

    private AuditLog()
    {
        Id = Guid.NewGuid();
        Action = string.Empty;
        Resource = string.Empty;
        IpAddress = string.Empty;
        Success = true;
        TimestampUtc = DateTime.UtcNow;
    }

    public AuditLog(
        Guid userId,
        string action,
        string resource,
        string ipAddress,
        bool success = true,
        string? resourceId = null,
        string? details = null,
        string? userAgent = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty", nameof(userId));
        
        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentException("Action cannot be null or empty", nameof(action));
        
        if (string.IsNullOrWhiteSpace(resource))
            throw new ArgumentException("Resource cannot be null or empty", nameof(resource));
        
        if (string.IsNullOrWhiteSpace(ipAddress))
            throw new ArgumentException("IpAddress cannot be null or empty", nameof(ipAddress));

        Id = Guid.NewGuid();
        UserId = userId;
        Action = action;
        Resource = resource;
        ResourceId = resourceId;
        Details = details;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        Success = success;
        TimestampUtc = DateTime.UtcNow;
    }

    public static AuditLog CreateSuccessLog(
        Guid userId,
        string action,
        string resource,
        string ipAddress,
        string? resourceId = null,
        string? details = null,
        string? userAgent = null)
    {
        return new AuditLog(userId, action, resource, ipAddress, true, resourceId, details, userAgent);
    }

    public static AuditLog CreateFailureLog(
        Guid userId,
        string action,
        string resource,
        string ipAddress,
        string? resourceId = null,
        string? details = null,
        string? userAgent = null)
    {
        return new AuditLog(userId, action, resource, ipAddress, false, resourceId, details, userAgent);
    }
}