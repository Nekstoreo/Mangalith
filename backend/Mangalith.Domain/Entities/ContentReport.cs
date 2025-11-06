using Mangalith.Domain.Enums;

namespace Mangalith.Domain.Entities;

public class ContentReport
{
    public Guid Id { get; private set; }
    public Guid PublicationId { get; private set; }
    public Guid ReportedByUserId { get; private set; }
    public ContentReportCategory Category { get; private set; }
    public string Description { get; private set; }
    public ContentReportStatus Status { get; private set; }
    public string? ModeratorResponse { get; private set; }
    public Guid? ReviewedByUserId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? ReviewedAtUtc { get; private set; }

    // Propiedades de navegación
    public Publication Publication { get; private set; } = null!;
    public User ReportedByUser { get; private set; } = null!;
    public User? ReviewedByUser { get; private set; }

    private ContentReport()
    {
        Id = Guid.Empty;
        PublicationId = Guid.Empty;
        ReportedByUserId = Guid.Empty;
        Category = ContentReportCategory.Other;
        Description = string.Empty;
        Status = ContentReportStatus.Pending;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public ContentReport(
        Guid publicationId,
        Guid reportedByUserId,
        ContentReportCategory category,
        string description)
    {
        Id = Guid.NewGuid();
        PublicationId = publicationId;
        ReportedByUserId = reportedByUserId;
        Category = category;
        Description = description;
        Status = ContentReportStatus.Pending;
        CreatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Marca el reporte como bajo revisión.
    /// </summary>
    public void MarkUnderReview(Guid moderatorId)
    {
        if (Status != ContentReportStatus.Pending)
        {
            throw new InvalidOperationException($"Cannot mark report as under review with status {Status}");
        }

        Status = ContentReportStatus.UnderReview;
        ReviewedByUserId = moderatorId;
        ReviewedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Resuelve el reporte con una respuesta del moderador.
    /// </summary>
    public void Resolve(Guid moderatorId, string response)
    {
        Status = ContentReportStatus.Resolved;
        ModeratorResponse = response;
        ReviewedByUserId = moderatorId;
        ReviewedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Desestima el reporte con una respuesta del moderador.
    /// </summary>
    public void Dismiss(Guid moderatorId, string response)
    {
        Status = ContentReportStatus.Dismissed;
        ModeratorResponse = response;
        ReviewedByUserId = moderatorId;
        ReviewedAtUtc = DateTime.UtcNow;
    }
}
