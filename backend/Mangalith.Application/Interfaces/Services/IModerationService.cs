using Mangalith.Application.Common.Models;
using Mangalith.Domain.Entities;
using Mangalith.Domain.Enums;

namespace Mangalith.Application.Interfaces.Services;

public interface IModerationService
{
    Task<PagedResult<Publication>> GetModerationQueueAsync(int page, int pageSize, PublicationStatus? status = null, CancellationToken cancellationToken = default);
    Task<ModerationAction> CreateModerationActionAsync(Guid publicationId, Guid moderatorId, ModerationActionType actionType, string comments, CancellationToken cancellationToken = default);
    Task<PagedResult<ModerationAction>> GetModerationHistoryAsync(Guid publicationId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PagedResult<ModerationAction>> GetModeratorActionsAsync(Guid moderatorId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task BulkModerationActionAsync(IEnumerable<Guid> publicationIds, ModerationActionType actionType, Guid moderatorId, string comments, CancellationToken cancellationToken = default);
}
