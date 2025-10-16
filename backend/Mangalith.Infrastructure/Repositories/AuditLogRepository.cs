using Microsoft.EntityFrameworkCore;
using Mangalith.Application.Common.Models;
using Mangalith.Application.Interfaces.Repositories;
using Mangalith.Domain.Entities;
using Mangalith.Infrastructure.Data;

namespace Mangalith.Infrastructure.Repositories;

/// <summary>
/// Repositorio Entity Framework para logs de auditoría
/// </summary>
public class AuditLogRepository : IAuditLogRepository
{
    private readonly MangalithDbContext _context;

    public AuditLogRepository(MangalithDbContext context)
    {
        _context = context;
    }

    public async Task<AuditLog> CreateAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync(cancellationToken);
        return auditLog;
    }

    public async Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<PagedResult<AuditLog>> GetPagedAsync(AuditLogFilter filter, CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(filter);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Include(a => a.User)
            .ToListAsync(cancellationToken);

        return new PagedResult<AuditLog>(items, totalCount, filter.Page, filter.PageSize);
    }

    public async Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId, int limit = 10, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.TimestampUtc)
            .Take(limit)
            .Include(a => a.User)
            .ToListAsync(cancellationToken);
    }

    public async Task<long> CountAsync(AuditLogFilter filter, CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(filter);
        return await query.CountAsync(cancellationToken);
    }

    public async Task<AuditLogStatistics> GetStatisticsAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs
            .Where(a => a.TimestampUtc >= fromDate && a.TimestampUtc <= toDate);

        var totalLogs = await query.CountAsync(cancellationToken);
        var successfulLogs = await query.CountAsync(a => a.Success, cancellationToken);
        var failedLogs = totalLogs - successfulLogs;
        var uniqueUsers = await query.Select(a => a.UserId).Distinct().CountAsync(cancellationToken);
        var uniqueIpAddresses = await query.Select(a => a.IpAddress).Distinct().CountAsync(cancellationToken);

        // Top actions
        var topActions = await query
            .GroupBy(a => a.Action)
            .Select(g => new { Action = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(10)
            .ToDictionaryAsync(x => x.Action, x => (long)x.Count, cancellationToken);

        // Top resources
        var topResources = await query
            .GroupBy(a => a.Resource)
            .Select(g => new { Resource = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(10)
            .ToDictionaryAsync(x => x.Resource, x => (long)x.Count, cancellationToken);

        // Top users (with email)
        var topUsers = await query
            .Include(a => a.User)
            .GroupBy(a => new { a.UserId, a.User.Email })
            .Select(g => new { UserEmail = g.Key.Email ?? g.Key.UserId.ToString(), Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(10)
            .ToDictionaryAsync(x => x.UserEmail, x => (long)x.Count, cancellationToken);

        // Logs by day
        var logsByDay = await query
            .GroupBy(a => a.TimestampUtc.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .OrderBy(x => x.Date)
            .ToDictionaryAsync(x => x.Date, x => (long)x.Count, cancellationToken);

        // Logs by hour
        var logsByHour = await query
            .GroupBy(a => a.TimestampUtc.Hour)
            .Select(g => new { Hour = g.Key, Count = g.Count() })
            .OrderBy(x => x.Hour)
            .ToDictionaryAsync(x => x.Hour, x => (long)x.Count, cancellationToken);

        return new AuditLogStatistics
        {
            TotalLogs = totalLogs,
            SuccessfulLogs = successfulLogs,
            FailedLogs = failedLogs,
            UniqueUsers = uniqueUsers,
            UniqueIpAddresses = uniqueIpAddresses,
            TopActions = topActions,
            TopResources = topResources,
            TopUsers = topUsers,
            LogsByDay = logsByDay,
            LogsByHour = logsByHour,
            FromDate = fromDate,
            ToDate = toDate,
            GeneratedAtUtc = DateTime.UtcNow
        };
    }

    public async Task<long> DeleteOlderThanAsync(DateTime cutoffDate, CancellationToken cancellationToken = default)
    {
        var oldLogs = _context.AuditLogs.Where(a => a.TimestampUtc < cutoffDate);
        var count = await oldLogs.CountAsync(cancellationToken);
        
        _context.AuditLogs.RemoveRange(oldLogs);
        await _context.SaveChangesAsync(cancellationToken);
        
        return count;
    }

    public async Task<IEnumerable<AuditLog>> GetAllAsync(AuditLogFilter filter, CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(filter);
        
        return await query
            .Include(a => a.User)
            .ToListAsync(cancellationToken);
    }

    private IQueryable<AuditLog> BuildQuery(AuditLogFilter filter)
    {
        var query = _context.AuditLogs.AsQueryable();

        // Filtros
        if (filter.UserId.HasValue)
            query = query.Where(a => a.UserId == filter.UserId.Value);

        if (!string.IsNullOrWhiteSpace(filter.Action))
            query = query.Where(a => a.Action.Contains(filter.Action));

        if (!string.IsNullOrWhiteSpace(filter.Resource))
            query = query.Where(a => a.Resource.Contains(filter.Resource));

        if (!string.IsNullOrWhiteSpace(filter.ResourceId))
            query = query.Where(a => a.ResourceId != null && a.ResourceId.Contains(filter.ResourceId));

        if (filter.Success.HasValue)
            query = query.Where(a => a.Success == filter.Success.Value);

        if (filter.FromDate.HasValue)
            query = query.Where(a => a.TimestampUtc >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            query = query.Where(a => a.TimestampUtc <= filter.ToDate.Value);

        if (!string.IsNullOrWhiteSpace(filter.IpAddress))
            query = query.Where(a => a.IpAddress.Contains(filter.IpAddress));

        // Ordenamiento
        query = filter.OrderBy.ToLowerInvariant() switch
        {
            "action" => filter.OrderDescending ? query.OrderByDescending(a => a.Action) : query.OrderBy(a => a.Action),
            "resource" => filter.OrderDescending ? query.OrderByDescending(a => a.Resource) : query.OrderBy(a => a.Resource),
            "success" => filter.OrderDescending ? query.OrderByDescending(a => a.Success) : query.OrderBy(a => a.Success),
            "ipaddress" => filter.OrderDescending ? query.OrderByDescending(a => a.IpAddress) : query.OrderBy(a => a.IpAddress),
            "userid" => filter.OrderDescending ? query.OrderByDescending(a => a.UserId) : query.OrderBy(a => a.UserId),
            _ => filter.OrderDescending ? query.OrderByDescending(a => a.TimestampUtc) : query.OrderBy(a => a.TimestampUtc)
        };

        return query;
    }
}