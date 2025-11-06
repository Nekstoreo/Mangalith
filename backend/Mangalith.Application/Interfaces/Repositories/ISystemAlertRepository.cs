using Mangalith.Domain.Entities;
using Mangalith.Domain.Enums;

namespace Mangalith.Application.Interfaces.Repositories;

public interface ISystemAlertRepository
{
    Task<SystemAlert?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<SystemAlert>> GetActiveAlertsAsync(CancellationToken cancellationToken = default);
    Task<List<SystemAlert>> GetAllAlertsAsync(bool includeResolved = false, CancellationToken cancellationToken = default);
    Task<SystemAlert> CreateAsync(SystemAlert alert, CancellationToken cancellationToken = default);
    Task<SystemAlert> UpdateAsync(SystemAlert alert, CancellationToken cancellationToken = default);
    Task<bool> ResolveAlertAsync(Guid alertId, CancellationToken cancellationToken = default);
    Task<int> GetActiveAlertCountAsync(AlertSeverity? severity = null, CancellationToken cancellationToken = default);
}