using Microsoft.EntityFrameworkCore;
using Mangalith.Application.Interfaces.Repositories;
using Mangalith.Domain.Entities;
using Mangalith.Domain.Enums;
using Mangalith.Infrastructure.Data;

namespace Mangalith.Infrastructure.Repositories;

public class SystemAlertRepository : ISystemAlertRepository
{
    private readonly MangalithDbContext _context;

    public SystemAlertRepository(MangalithDbContext context)
    {
        _context = context;
    }

    public async Task<SystemAlert?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.SystemAlerts
            .FirstOrDefaultAsync(sa => sa.Id == id, cancellationToken);
    }

    public async Task<List<SystemAlert>> GetActiveAlertsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SystemAlerts
            .Where(sa => !sa.IsResolved)
            .OrderByDescending(sa => sa.Severity)
            .ThenByDescending(sa => sa.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<SystemAlert>> GetAllAlertsAsync(bool includeResolved = false, CancellationToken cancellationToken = default)
    {
        var query = _context.SystemAlerts.AsQueryable();

        if (!includeResolved)
        {
            query = query.Where(sa => !sa.IsResolved);
        }

        return await query
            .OrderByDescending(sa => sa.Severity)
            .ThenByDescending(sa => sa.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<SystemAlert> CreateAsync(SystemAlert alert, CancellationToken cancellationToken = default)
    {
        await _context.SystemAlerts.AddAsync(alert, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return alert;
    }

    public async Task<SystemAlert> UpdateAsync(SystemAlert alert, CancellationToken cancellationToken = default)
    {
        _context.SystemAlerts.Update(alert);
        await _context.SaveChangesAsync(cancellationToken);
        return alert;
    }

    public async Task<bool> ResolveAlertAsync(Guid alertId, CancellationToken cancellationToken = default)
    {
        var alert = await GetByIdAsync(alertId, cancellationToken);
        if (alert == null) return false;

        alert.Resolve("System");
        await UpdateAsync(alert, cancellationToken);
        return true;
    }

    public async Task<int> GetActiveAlertCountAsync(AlertSeverity? severity = null, CancellationToken cancellationToken = default)
    {
        var query = _context.SystemAlerts
            .Where(sa => !sa.IsResolved);

        if (severity.HasValue)
        {
            query = query.Where(sa => sa.Severity == severity.Value);
        }

        return await query.CountAsync(cancellationToken);
    }
}