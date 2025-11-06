using Mangalith.Domain.Enums;

namespace Mangalith.Domain.Entities;

public class SystemAlert
{
    public Guid Id { get; private set; }
    public AlertType Type { get; private set; }
    public AlertSeverity Severity { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Metadata { get; private set; } = "{}"; // JSON serialized
    public DateTime CreatedAt { get; private set; }
    public bool IsResolved { get; private set; }
    public DateTime? ResolvedAt { get; private set; }
    public string? ResolvedBy { get; private set; }

    private SystemAlert() { } // EF Constructor

    public SystemAlert(
        AlertType type,
        AlertSeverity severity,
        string title,
        string description,
        string metadata = "{}")
    {
        Id = Guid.NewGuid();
        Type = type;
        Severity = severity;
        Title = title;
        Description = description;
        Metadata = metadata;
        CreatedAt = DateTime.UtcNow;
        IsResolved = false;
    }

    public void Resolve(string resolvedBy)
    {
        IsResolved = true;
        ResolvedAt = DateTime.UtcNow;
        ResolvedBy = resolvedBy;
    }

    public void UpdateSeverity(AlertSeverity newSeverity)
    {
        Severity = newSeverity;
    }
}