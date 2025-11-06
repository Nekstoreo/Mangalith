using Mangalith.Domain.Enums;

namespace Mangalith.Domain.Entities;

public class ModerationAction
{
    public Guid Id { get; private set; }
    public Guid PublicationId { get; private set; }
    public Guid ModeratorId { get; private set; }
    public ModerationActionType ActionType { get; private set; }
    public string Comments { get; private set; }
    public PublicationStatus PreviousStatus { get; private set; }
    public PublicationStatus NewStatus { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    // Propiedades de navegación
    public Publication Publication { get; private set; } = null!;
    public User Moderator { get; private set; } = null!;

    private ModerationAction()
    {
        Id = Guid.Empty;
        PublicationId = Guid.Empty;
        ModeratorId = Guid.Empty;
        ActionType = ModerationActionType.Submitted;
        Comments = string.Empty;
        PreviousStatus = PublicationStatus.Draft;
        NewStatus = PublicationStatus.Draft;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public ModerationAction(
        Guid publicationId,
        Guid moderatorId,
        ModerationActionType actionType,
        string comments,
        PublicationStatus previousStatus,
        PublicationStatus newStatus)
    {
        Id = Guid.NewGuid();
        PublicationId = publicationId;
        ModeratorId = moderatorId;
        ActionType = actionType;
        Comments = comments;
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
        CreatedAtUtc = DateTime.UtcNow;
    }
}
