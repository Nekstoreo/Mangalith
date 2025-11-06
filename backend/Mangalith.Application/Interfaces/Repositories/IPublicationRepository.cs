using Mangalith.Application.Common.Models;
using Mangalith.Domain.Entities;
using Mangalith.Domain.Enums;

namespace Mangalith.Application.Interfaces.Repositories;

public interface IPublicationRepository
{
    Task<Publication?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Publication?> GetByMangaIdAsync(Guid mangaId, CancellationToken cancellationToken = default);
    Task<PagedResult<Publication>> GetByStatusAsync(PublicationStatus status, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PagedResult<Publication>> GetByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Publication> CreateAsync(Publication publication, CancellationToken cancellationToken = default);
    Task<Publication> UpdateAsync(Publication publication, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> GetCountByStatusAsync(PublicationStatus status, CancellationToken cancellationToken = default);
    Task<PagedResult<Publication>> GetModerationQueueAsync(int page, int pageSize, PublicationStatus? status = null, CancellationToken cancellationToken = default);
    
    // Analytics methods
    Task<Dictionary<PublicationStatus, int>> GetStatusDistributionAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
    Task<Dictionary<ContentRating, int>> GetContentRatingDistributionAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
    Task<List<PublicationTrend>> GetPublicationTrendsAsync(int days = 30, CancellationToken cancellationToken = default);
    Task<double> GetAverageReviewTimeAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
    Task<Dictionary<string, int>> GetTopCreatorsAsync(int limit = 10, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
    Task<int> GetSubmissionCountAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
}
