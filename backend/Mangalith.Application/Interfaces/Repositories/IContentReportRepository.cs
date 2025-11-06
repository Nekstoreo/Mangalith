using Mangalith.Application.Common.Models;
using Mangalith.Domain.Entities;
using Mangalith.Domain.Enums;

namespace Mangalith.Application.Interfaces.Repositories;

public interface IContentReportRepository
{
    Task<ContentReport?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<ContentReport>> GetByPublicationIdAsync(Guid publicationId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PagedResult<ContentReport>> GetByStatusAsync(ContentReportStatus status, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PagedResult<ContentReport>> GetPendingAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PagedResult<ContentReport>> GetByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IEnumerable<ContentReport>> GetByPublicationAndUserAsync(Guid publicationId, Guid userId, CancellationToken cancellationToken = default);
    Task<ContentReport> CreateAsync(ContentReport report, CancellationToken cancellationToken = default);
    Task<ContentReport> UpdateAsync(ContentReport report, CancellationToken cancellationToken = default);
    Task<int> GetCountByPublicationIdAsync(Guid publicationId, CancellationToken cancellationToken = default);
    Task<int> GetCountByStatusAsync(ContentReportStatus status, CancellationToken cancellationToken = default);
    
    // Analytics methods
    Task<Dictionary<ContentReportCategory, int>> GetCategoryDistributionAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
    Task<int> GetReportCountAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
}
