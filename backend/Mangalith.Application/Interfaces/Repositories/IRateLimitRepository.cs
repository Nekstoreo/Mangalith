using Mangalith.Domain.Entities;

namespace Mangalith.Application.Interfaces.Repositories;

public interface IRateLimitRepository
{
    Task<RateLimitEntry?> GetByUserAndEndpointAsync(Guid userId, string endpoint, CancellationToken cancellationToken = default);
    Task<RateLimitEntry> CreateAsync(RateLimitEntry rateLimitEntry, CancellationToken cancellationToken = default);
    Task<RateLimitEntry> UpdateAsync(RateLimitEntry rateLimitEntry, CancellationToken cancellationToken = default);
    Task DeleteExpiredEntriesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<RateLimitEntry>> GetActiveEntriesForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Dictionary<string, int>> GetEndpointUsageStatsAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<RateLimitEntry>> GetRecentEntriesAsync(DateTime fromDate, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
}