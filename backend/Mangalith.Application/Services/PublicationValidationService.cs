using Mangalith.Application.Common.Exceptions;
using Mangalith.Application.Interfaces.Repositories;
using Mangalith.Application.Interfaces.Services;
using Mangalith.Domain.Entities;
using Mangalith.Domain.Enums;

namespace Mangalith.Application.Services;

/// <summary>
/// Service for validating publication workflow operations
/// </summary>
public class PublicationValidationService : IPublicationValidationService
{
    private readonly IMangaRepository _mangaRepository;
    private readonly IUserRepository _userRepository;
    private readonly IChapterRepository _chapterRepository;

    public PublicationValidationService(
        IMangaRepository mangaRepository,
        IUserRepository userRepository,
        IChapterRepository chapterRepository)
    {
        _mangaRepository = mangaRepository;
        _userRepository = userRepository;
        _chapterRepository = chapterRepository;
    }

    /// <summary>
    /// Validates that a publication can be created for the given manga
    /// </summary>
    public async Task ValidatePublicationCreationAsync(Guid mangaId, Guid userId, CancellationToken cancellationToken = default)
    {
        // Verify manga exists
        var manga = await _mangaRepository.GetByIdAsync(mangaId, cancellationToken);
        if (manga == null)
        {
            throw new PublicationValidationException("MANGA_NOT_FOUND", $"Manga with ID {mangaId} not found");
        }

        // Verify user owns the manga
        if (manga.CreatedByUserId != userId)
        {
            throw new PublicationValidationException("UNAUTHORIZED_MANGA_ACCESS", "You can only create publications for your own manga");
        }

        // Verify manga has basic required information
        if (string.IsNullOrWhiteSpace(manga.Title))
        {
            throw new PublicationValidationException("MISSING_MANGA_TITLE", "Manga must have a title before publication");
        }

        if (string.IsNullOrWhiteSpace(manga.Description))
        {
            throw new PublicationValidationException("MISSING_MANGA_DESCRIPTION", "Manga must have a description before publication");
        }
    }

    /// <summary>
    /// Validates that a publication can be submitted for review
    /// </summary>
    public async Task ValidatePublicationSubmissionAsync(Publication publication, CancellationToken cancellationToken = default)
    {
        // Verify publication can transition to InReview
        if (!publication.CanTransitionTo(PublicationStatus.InReview))
        {
            throw new InvalidPublicationStateException(publication.Status, PublicationStatus.InReview);
        }

        // Verify manga has at least one chapter
        var manga = await _mangaRepository.GetByIdAsync(publication.MangaId, cancellationToken);
        if (manga?.Chapters == null || !manga.Chapters.Any())
        {
            throw new PublicationValidationException("NO_CHAPTERS", "Cannot submit publication without at least one chapter");
        }

        // Verify chapters have pages
        // Nota: consideramos válido si al menos un capítulo reporta PageCount > 0,
        // para evitar depender de la carga explícita de la colección Pages.
        var hasValidChapters = false;
        foreach (var chapter in manga.Chapters)
        {
            var chapterWithPages = await _chapterRepository.GetByIdAsync(chapter.Id, cancellationToken);
            if ((chapterWithPages?.Pages?.Any() ?? false) || (chapterWithPages?.PageCount ?? 0) > 0)
            {
                hasValidChapters = true;
                break;
            }
        }

        if (!hasValidChapters)
        {
            throw new PublicationValidationException("NO_VALID_CHAPTERS", "Cannot submit publication without at least one chapter with pages");
        }
    }

    /// <summary>
    /// Validates that a user can perform moderation actions
    /// </summary>
    public async Task ValidateModeratorPermissionsAsync(Guid moderatorId, CancellationToken cancellationToken = default)
    {
        var moderator = await _userRepository.GetByIdAsync(moderatorId, cancellationToken);
        if (moderator == null)
        {
            throw new ModerationValidationException("MODERATOR_NOT_FOUND", $"Moderator with ID {moderatorId} not found");
        }

        if (moderator.Role != UserRole.Moderator && moderator.Role != UserRole.Administrator)
        {
            throw new ModerationValidationException("INSUFFICIENT_PERMISSIONS", "Only moderators and administrators can perform moderation actions");
        }
    }

    /// <summary>
    /// Validates moderation action parameters
    /// </summary>
    public void ValidateModerationAction(PublicationStatus currentStatus, PublicationStatus targetStatus, string? comments, string? reason = null)
    {
        // Validate state transition
        if (!IsValidStateTransition(currentStatus, targetStatus))
        {
            throw new InvalidPublicationStateException(currentStatus, targetStatus);
        }

        // Validate required comments for certain actions
        if (targetStatus == PublicationStatus.Rejected && string.IsNullOrWhiteSpace(reason))
        {
            throw new ModerationValidationException("MISSING_REJECTION_REASON", "Rejection reason is required when rejecting publications");
        }

        if (targetStatus == PublicationStatus.NeedsRevision && string.IsNullOrWhiteSpace(comments))
        {
            throw new ModerationValidationException("MISSING_REVISION_COMMENTS", "Comments are required when requesting revisions");
        }

        // Validate comment length
        if (!string.IsNullOrEmpty(comments) && comments.Length > 2000)
        {
            throw new ModerationValidationException("COMMENTS_TOO_LONG", "Moderation comments cannot exceed 2000 characters");
        }

        if (!string.IsNullOrEmpty(reason) && reason.Length > 500)
        {
            throw new ModerationValidationException("REASON_TOO_LONG", "Rejection reason cannot exceed 500 characters");
        }
    }

    /// <summary>
    /// Validates content report creation
    /// </summary>
    public async Task ValidateContentReportAsync(Guid publicationId, Guid reporterId, string description, CancellationToken cancellationToken = default)
    {
        // Verify publication exists and is published
        var publication = await _mangaRepository.GetByIdAsync(publicationId, cancellationToken);
        if (publication == null)
        {
            throw new ContentReportValidationException("PUBLICATION_NOT_FOUND", $"Publication with ID {publicationId} not found");
        }

        // Verify reporter exists
        var reporter = await _userRepository.GetByIdAsync(reporterId, cancellationToken);
        if (reporter == null)
        {
            throw new ContentReportValidationException("REPORTER_NOT_FOUND", $"User with ID {reporterId} not found");
        }

        // Validate description
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ContentReportValidationException("MISSING_DESCRIPTION", "Report description is required");
        }

        if (description.Length < 10)
        {
            throw new ContentReportValidationException("DESCRIPTION_TOO_SHORT", "Report description must be at least 10 characters");
        }

        if (description.Length > 1000)
        {
            throw new ContentReportValidationException("DESCRIPTION_TOO_LONG", "Report description cannot exceed 1000 characters");
        }
    }

    /// <summary>
    /// Validates bulk moderation actions
    /// </summary>
    public void ValidateBulkModerationAction(IEnumerable<Guid> publicationIds, string comments)
    {
        var publicationList = publicationIds.ToList();
        
        if (!publicationList.Any())
        {
            throw new ModerationValidationException("NO_PUBLICATIONS_SELECTED", "At least one publication must be selected for bulk actions");
        }

        if (publicationList.Count > 100)
        {
            throw new ModerationValidationException("TOO_MANY_PUBLICATIONS", "Bulk actions are limited to 100 publications at a time");
        }

        if (string.IsNullOrWhiteSpace(comments))
        {
            throw new ModerationValidationException("MISSING_BULK_COMMENTS", "Comments are required for bulk moderation actions");
        }

        if (comments.Length > 2000)
        {
            throw new ModerationValidationException("BULK_COMMENTS_TOO_LONG", "Bulk action comments cannot exceed 2000 characters");
        }
    }

    private static bool IsValidStateTransition(PublicationStatus from, PublicationStatus to)
    {
        return from switch
        {
            PublicationStatus.Draft => to is PublicationStatus.InReview or PublicationStatus.Archived,
            PublicationStatus.InReview => to is PublicationStatus.Published or PublicationStatus.Rejected or PublicationStatus.NeedsRevision,
            PublicationStatus.NeedsRevision => to is PublicationStatus.InReview or PublicationStatus.Draft or PublicationStatus.Archived,
            PublicationStatus.Rejected => to is PublicationStatus.Draft or PublicationStatus.Archived,
            PublicationStatus.Published => to is PublicationStatus.Archived or PublicationStatus.UnderReview,
            PublicationStatus.UnderReview => to is PublicationStatus.Published or PublicationStatus.Archived,
            PublicationStatus.Archived => false, // Archived is final state
            _ => false
        };
    }
}