using Mangalith.Application.Common.Models;
using Mangalith.Domain.Entities;
using Mangalith.Domain.Enums;

namespace Mangalith.Application.Interfaces.Services;

public interface IContentReportService
{
    Task<ContentReport> CreateReportAsync(Guid publicationId, Guid reportedByUserId, ContentReportCategory category, string description, CancellationToken cancellationToken = default);
    Task<ContentReport> ReviewReportAsync(Guid reportId, Guid moderatorId, ContentReportStatus status, string? response = null, CancellationToken cancellationToken = default);
    Task<PagedResult<ContentReport>> GetPendingReportsAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PagedResult<ContentReport>> GetReportsByPublicationAsync(Guid publicationId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetReportCountByPublicationAsync(Guid publicationId, CancellationToken cancellationToken = default);
    Task<PagedResult<ContentReport>> GetUserReportsAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
}
