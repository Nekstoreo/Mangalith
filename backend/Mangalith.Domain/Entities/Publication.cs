using Mangalith.Domain.Enums;

namespace Mangalith.Domain.Entities;

public class Publication
{
    public Guid Id { get; private set; }
    public Guid MangaId { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public PublicationStatus Status { get; private set; }
    public ContentRating ContentRating { get; private set; }
    public bool IsNsfw { get; private set; }
    public string? ModeratorComments { get; private set; }
    public string? RejectionReason { get; private set; }
    public DateTime? SubmittedAtUtc { get; private set; }
    public DateTime? ReviewedAtUtc { get; private set; }
    public Guid? ReviewedByUserId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    // Propiedades de navegación
    public Manga Manga { get; private set; } = null!;
    public User CreatedByUser { get; private set; } = null!;
    public User? ReviewedByUser { get; private set; }
    public ICollection<ModerationAction> ModerationActions { get; private set; } = new List<ModerationAction>();
    public ICollection<ContentReport> Reports { get; private set; } = new List<ContentReport>();

    private Publication()
    {
        Id = Guid.Empty;
        MangaId = Guid.Empty;
        CreatedByUserId = Guid.Empty;
        Status = PublicationStatus.Draft;
        ContentRating = ContentRating.General;
        IsNsfw = false;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public Publication(Guid mangaId, Guid createdByUserId)
    {
        Id = Guid.NewGuid();
        MangaId = mangaId;
        CreatedByUserId = createdByUserId;
        Status = PublicationStatus.Draft;
        ContentRating = ContentRating.General;
        IsNsfw = false;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Cambia el estado de publicación a InReview y registra el timestamp de envío.
    /// </summary>
    public void SubmitForReview()
    {
        if (Status != PublicationStatus.Draft && Status != PublicationStatus.NeedsRevision)
        {
            throw new InvalidOperationException($"Cannot submit publication with status {Status}");
        }

        Status = PublicationStatus.InReview;
        SubmittedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Aprueba la publicación con clasificación de contenido y comentarios.
    /// </summary>
    public void Approve(Guid reviewedByUserId, ContentRating rating, bool isNsfw, string? comments = null)
    {
        if (Status != PublicationStatus.InReview)
        {
            throw new InvalidOperationException($"Cannot approve publication with status {Status}");
        }

        Status = PublicationStatus.Published;
        ContentRating = rating;
        IsNsfw = isNsfw;
        ModeratorComments = comments;
        ReviewedByUserId = reviewedByUserId;
        ReviewedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Rechaza la publicación con razón y comentarios.
    /// </summary>
    public void Reject(Guid reviewedByUserId, string reason, string comments)
    {
        if (Status != PublicationStatus.InReview)
        {
            throw new InvalidOperationException($"Cannot reject publication with status {Status}");
        }

        Status = PublicationStatus.Rejected;
        RejectionReason = reason;
        ModeratorComments = comments;
        ReviewedByUserId = reviewedByUserId;
        ReviewedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Solicita revisión de la publicación con comentarios específicos.
    /// </summary>
    public void RequestRevision(Guid reviewedByUserId, string comments)
    {
        if (Status != PublicationStatus.InReview)
        {
            throw new InvalidOperationException($"Cannot request revision for publication with status {Status}");
        }

        Status = PublicationStatus.NeedsRevision;
        ModeratorComments = comments;
        ReviewedByUserId = reviewedByUserId;
        ReviewedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Archiva la publicación (solo desde estados publicados o bajo revisión).
    /// </summary>
    public void Archive()
    {
        if (Status != PublicationStatus.Published && Status != PublicationStatus.UnderReview)
        {
            throw new InvalidOperationException($"Cannot archive publication with status {Status}");
        }

        Status = PublicationStatus.Archived;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Marca la publicación como bajo revisión por reporte de contenido.
    /// </summary>
    public void MarkUnderReview()
    {
        if (Status != PublicationStatus.Published)
        {
            throw new InvalidOperationException($"Cannot mark publication as under review with status {Status}");
        }

        Status = PublicationStatus.UnderReview;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Valida las transiciones de estado posibles.
    /// </summary>
    public bool CanTransitionTo(PublicationStatus targetStatus)
    {
        return (Status, targetStatus) switch
        {
            (PublicationStatus.Draft, PublicationStatus.InReview) => true,
            (PublicationStatus.Draft, PublicationStatus.Archived) => true,
            (PublicationStatus.InReview, PublicationStatus.Published) => true,
            (PublicationStatus.InReview, PublicationStatus.Rejected) => true,
            (PublicationStatus.InReview, PublicationStatus.NeedsRevision) => true,
            (PublicationStatus.NeedsRevision, PublicationStatus.InReview) => true,
            (PublicationStatus.NeedsRevision, PublicationStatus.Draft) => true,
            (PublicationStatus.NeedsRevision, PublicationStatus.Archived) => true,
            (PublicationStatus.Rejected, PublicationStatus.Draft) => true,
            (PublicationStatus.Rejected, PublicationStatus.Archived) => true,
            (PublicationStatus.Published, PublicationStatus.Archived) => true,
            (PublicationStatus.Published, PublicationStatus.UnderReview) => true,
            (PublicationStatus.UnderReview, PublicationStatus.Published) => true,
            (PublicationStatus.UnderReview, PublicationStatus.Archived) => true,
            _ => false
        };
    }
}
