using Mangalith.Domain.Entities;

namespace Mangalith.Application.Interfaces.Repositories;

public interface IUserQuotaRepository
{
    Task<UserQuota?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserQuota> CreateAsync(UserQuota userQuota, CancellationToken cancellationToken = default);
    Task<UserQuota> UpdateAsync(UserQuota userQuota, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserQuota>> GetUsersNearLimitAsync(double thresholdPercentage = 80.0, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserQuota>> GetUsersExceedingLimitAsync(CancellationToken cancellationToken = default);
    Task<long> GetTotalStorageUsageAsync(CancellationToken cancellationToken = default);
    Task<Dictionary<UserRole, long>> GetStorageUsageByRoleAsync(CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
}