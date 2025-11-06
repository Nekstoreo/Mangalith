using Mangalith.Application.Common.Exceptions;
using Mangalith.Application.Common.Models;
using Mangalith.Application.Interfaces.Repositories;
using Mangalith.Application.Interfaces.Services;
using Mangalith.Domain.Entities;
using Mangalith.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Mangalith.Application.Services;

public class ModerationService : IModerationService
{
    private readonly IPublicationRepository _publicationRepository;
    private readonly IModerationActionRepository _moderationActionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPublicationService _publicationService;
    private readonly PublicationValidationService _validationService;
    private readonly ILogger<ModerationService> _logger;

    public ModerationService(
        IPublicationRepository publicationRepository,
        IModerationActionRepository moderationActionRepository,
        IUserRepository userRepository,
        IPublicationService publicationService,
        PublicationValidationService validationService,
        ILogger<ModerationService> logger)
    {
        _publicationRepository = publicationRepository;
        _moderationActionRepository = moderationActionRepository;
        _userRepository = userRepository;
        _publicationService = publicationService;
        _validationService = validationService;
        _logger = logger;
    }

    public async Task<PagedResult<Publication>> GetModerationQueueAsync(int page, int pageSize, PublicationStatus? status = null, CancellationToken cancellationToken = default)
    {
        return await _publicationRepository.GetModerationQueueAsync(page, pageSize, status, cancellationToken);
    }

    public async Task<ModerationAction> CreateModerationActionAsync(Guid publicationId, Guid moderatorId, ModerationActionType actionType, string comments, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating moderation action {ActionType} for publication {PublicationId} by moderator {ModeratorId}", 
            actionType, publicationId, moderatorId);

        var publication = await _publicationRepository.GetByIdAsync(publicationId, cancellationToken)
            ?? throw new NotFoundException($"Publication with ID {publicationId} not found");

        // Validate moderator permissions
        await _validationService.ValidateModeratorPermissionsAsync(moderatorId, cancellationToken);

        // Validate comments
        if (string.IsNullOrWhiteSpace(comments))
        {
            throw new ModerationValidationException("MISSING_COMMENTS", "Comments are required for moderation actions");
        }

        if (comments.Length > 2000)
        {
            throw new ModerationValidationException("COMMENTS_TOO_LONG", "Comments cannot exceed 2000 characters");
        }

        var action = new ModerationAction(
            publicationId,
            moderatorId,
            actionType,
            comments,
            publication.Status,
            publication.Status
        );

        var createdAction = await _moderationActionRepository.CreateAsync(action, cancellationToken);
        
        _logger.LogInformation("Moderation action {ActionId} created successfully for publication {PublicationId}", 
            createdAction.Id, publicationId);
        
        return createdAction;
    }

    public async Task<PagedResult<ModerationAction>> GetModerationHistoryAsync(Guid publicationId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        // Verificar que la publicación existe
        var publication = await _publicationRepository.GetByIdAsync(publicationId, cancellationToken)
            ?? throw new NotFoundException($"Publication with ID {publicationId} not found");

        return await _moderationActionRepository.GetByPublicationIdAsync(publicationId, page, pageSize, cancellationToken);
    }

    public async Task<PagedResult<ModerationAction>> GetModeratorActionsAsync(Guid moderatorId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        // Verificar que el moderador existe
        var moderator = await _userRepository.GetByIdAsync(moderatorId, cancellationToken)
            ?? throw new NotFoundException($"Moderator with ID {moderatorId} not found");

        return await _moderationActionRepository.GetByModeratorIdAsync(moderatorId, page, pageSize, cancellationToken);
    }

    public async Task BulkModerationActionAsync(IEnumerable<Guid> publicationIds, ModerationActionType actionType, Guid moderatorId, string comments, CancellationToken cancellationToken = default)
    {
        var ids = publicationIds.ToList();
        _logger.LogInformation("Starting bulk moderation action {ActionType} for {Count} publications by moderator {ModeratorId}", 
            actionType, ids.Count, moderatorId);

        // Validate bulk action parameters
        _validationService.ValidateBulkModerationAction(ids, comments);

        // Validate moderator permissions
        await _validationService.ValidateModeratorPermissionsAsync(moderatorId, cancellationToken);

        var successCount = 0;
        var failureCount = 0;

        // Procesar cada publicación
        foreach (var publicationId in ids)
        {
            try
            {
                var publication = await _publicationRepository.GetByIdAsync(publicationId, cancellationToken);
                if (publication == null)
                {
                    _logger.LogWarning("Publication {PublicationId} not found during bulk action", publicationId);
                    failureCount++;
                    continue;
                }

                // Crear acción de moderación
                var action = new ModerationAction(
                    publicationId,
                    moderatorId,
                    actionType,
                    comments,
                    publication.Status,
                    publication.Status
                );

                await _moderationActionRepository.CreateAsync(action, cancellationToken);
                successCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process publication {PublicationId} in bulk action", publicationId);
                failureCount++;
            }
        }

        _logger.LogInformation("Bulk moderation action completed. Success: {SuccessCount}, Failures: {FailureCount}", 
            successCount, failureCount);

        if (successCount == 0)
        {
            throw new ModerationException("BULK_ACTION_FAILED", "No publications were successfully processed");
        }
    }

    public async Task<ModerationStatistics> GetModerationStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating moderation statistics from {FromDate} to {ToDate}", fromDate, toDate);

        try
        {
            // Get publication counts by status
            var totalInReview = await _publicationRepository.GetCountByStatusAsync(PublicationStatus.InReview, cancellationToken);
            var totalNeedsRevision = await _publicationRepository.GetCountByStatusAsync(PublicationStatus.NeedsRevision, cancellationToken);
            var totalPublished = await _publicationRepository.GetCountByStatusAsync(PublicationStatus.Published, cancellationToken);
            var totalRejected = await _publicationRepository.GetCountByStatusAsync(PublicationStatus.Rejected, cancellationToken);

            // Get moderation actions in date range
            var actions = await _moderationActionRepository.GetActionsByDateRangeAsync(fromDate, toDate, cancellationToken);
            
            var approvalCount = actions.Count(a => a.ActionType == ModerationActionType.Approved);
            var rejectionCount = actions.Count(a => a.ActionType == ModerationActionType.Rejected);
            var revisionCount = actions.Count(a => a.ActionType == ModerationActionType.RequestedRevision);

            var totalActions = approvalCount + rejectionCount + revisionCount;
            var approvalRate = totalActions > 0 ? (double)approvalCount / totalActions : 0;

            return new ModerationStatistics
            {
                TotalInReview = totalInReview,
                TotalNeedsRevision = totalNeedsRevision,
                TotalPublished = totalPublished,
                TotalRejected = totalRejected,
                ApprovalRate = approvalRate,
                TotalActionsInPeriod = totalActions,
                ApprovalsInPeriod = approvalCount,
                RejectionsInPeriod = rejectionCount,
                RevisionsInPeriod = revisionCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate moderation statistics");
            throw new ModerationException("STATISTICS_GENERATION_FAILED", "Failed to generate moderation statistics");
        }
    }
}
