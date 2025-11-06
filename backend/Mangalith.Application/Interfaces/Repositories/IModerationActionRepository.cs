using Mangalith.Application.Common.Models;
using Mangalith.Domain.Entities;
using Mangalith.Domain.Enums;

namespace Mangalith.Application.Interfaces.Repositories;

public interface IModerationActionRepository
{
    Task<ModerationAction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<ModerationAction>> GetByPublicationIdAsync(Guid publicationId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PagedResult<ModerationAction>> GetByModeratorIdAsync(Guid moderatorId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<ModerationAction> CreateAsync(ModerationAction action, CancellationToken cancellationToken = default);
    Task<List<ModerationAction>> GetAllByPublicationIdAsync(Guid publicationId, CancellationToken cancellationToken = default);
    Task<List<ModerationAction>> GetActionsByDateRangeAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
    
    // Analytics methods
    Task<List<ModeratorPerformance>> GetModeratorPerformanceAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
    Task<Dictionary<ModerationActionType, int>> GetActionTypeDistributionAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
    Task<int> GetActionCountByModeratorAsync(Guid moderatorId, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
    Task<double> GetAverageActionTimeByModeratorAsync(Guid moderatorId, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
}
