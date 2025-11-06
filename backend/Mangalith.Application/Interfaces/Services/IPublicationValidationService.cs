using Mangalith.Domain.Entities;
using Mangalith.Domain.Enums;

namespace Mangalith.Application.Interfaces.Services;

/// <summary>
/// Interface for publication validation operations
/// </summary>
public interface IPublicationValidationService
{
    /// <summary>
    /// Validates that a publication can be created for the given manga
    /// </summary>
    Task ValidatePublicationCreationAsync(Guid mangaId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that a publication can be submitted for review
    /// </summary>
    Task ValidatePublicationSubmissionAsync(Publication publication, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that a user can perform moderation actions
    /// </summary>
    Task ValidateModeratorPermissionsAsync(Guid moderatorId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates moderation action parameters
    /// </summary>
    void ValidateModerationAction(PublicationStatus currentStatus, PublicationStatus targetStatus, string? comments, string? reason = null);

    /// <summary>
    /// Validates content report creation
    /// </summary>
    Task ValidateContentReportAsync(Guid publicationId, Guid reporterId, string description, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates bulk moderation actions
    /// </summary>
    void ValidateBulkModerationAction(IEnumerable<Guid> publicationIds, string comments);
}
