using Microsoft.EntityFrameworkCore;
using Mangalith.Application.Common.Models;
using Mangalith.Application.Interfaces.Repositories;
using Mangalith.Domain.Entities;
using Mangalith.Infrastructure.Data;

namespace Mangalith.Infrastructure.Repositories;

public class ModerationActionRepository : IModerationActionRepository
{
    private readonly MangalithDbContext _context;

    public ModerationActionRepository(MangalithDbContext context)
    {
        _context = context;
    }

    public async Task<ModerationAction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ModerationActions
            .Include(ma => ma.Publication)
            .Include(ma => ma.Moderator)
            .FirstOrDefaultAsync(ma => ma.Id == id, cancellationToken);
    }

    public async Task<PagedResult<ModerationAction>> GetByPublicationIdAsync(Guid publicationId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.ModerationActions
            .Where(ma => ma.PublicationId == publicationId)
            .Include(ma => ma.Moderator)
            .OrderByDescending(ma => ma.CreatedAtUtc);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<ModerationAction>(items, total, page, pageSize);
    }

    public async Task<PagedResult<ModerationAction>> GetByModeratorIdAsync(Guid moderatorId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.ModerationActions
            .Where(ma => ma.ModeratorId == moderatorId)
            .Include(ma => ma.Publication)
            .OrderByDescending(ma => ma.CreatedAtUtc);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<ModerationAction>(items, total, page, pageSize);
    }

    public async Task<ModerationAction> CreateAsync(ModerationAction action, CancellationToken cancellationToken = default)
    {
        await _context.ModerationActions.AddAsync(action, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return action;
    }

    public async Task<List<ModerationAction>> GetAllByPublicationIdAsync(Guid publicationId, CancellationToken cancellationToken = default)
    {
        return await _context.ModerationActions
            .Where(ma => ma.PublicationId == publicationId)
            .Include(ma => ma.Moderator)
            .OrderByDescending(ma => ma.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ModerationAction>> GetActionsByDateRangeAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.ModerationActions.AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(ma => ma.CreatedAtUtc >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(ma => ma.CreatedAtUtc <= toDate.Value);

        return await query
            .Include(ma => ma.Moderator)
            .Include(ma => ma.Publication)
            .OrderByDescending(ma => ma.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ModeratorPerformance>> GetModeratorPerformanceAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.ModerationActions
            .Include(ma => ma.Moderator)
            .AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(ma => ma.CreatedAtUtc >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(ma => ma.CreatedAtUtc <= toDate.Value);

        var moderatorStats = await query
            .GroupBy(ma => new { ma.ModeratorId, ma.Moderator.FullName })
            .Select(g => new
            {
                ModeratorId = g.Key.ModeratorId,
                ModeratorName = g.Key.FullName,
                ActionsCompleted = g.Count(),
                ApprovalsCount = g.Count(ma => ma.ActionType == Domain.Enums.ModerationActionType.Approved),
                RejectionsCount = g.Count(ma => ma.ActionType == Domain.Enums.ModerationActionType.Rejected),
                LastActiveAt = g.Max(ma => ma.CreatedAtUtc)
            })
            .ToListAsync(cancellationToken);

        var performances = new List<ModeratorPerformance>();

        foreach (var stat in moderatorStats)
        {
            // Calculate additional metrics
            var last7Days = DateTime.UtcNow.AddDays(-7);
            var last30Days = DateTime.UtcNow.AddDays(-30);

            var actionsLast7Days = await _context.ModerationActions
                .Where(ma => ma.ModeratorId == stat.ModeratorId && ma.CreatedAtUtc >= last7Days)
                .CountAsync(cancellationToken);

            var actionsLast30Days = await _context.ModerationActions
                .Where(ma => ma.ModeratorId == stat.ModeratorId && ma.CreatedAtUtc >= last30Days)
                .CountAsync(cancellationToken);

            // Calculate average review time (simplified - would need publication data for accurate calculation)
            var avgReviewTime = await _context.ModerationActions
                .Where(ma => ma.ModeratorId == stat.ModeratorId)
                .Join(_context.Publications, ma => ma.PublicationId, p => p.Id, (ma, p) => new { ma, p })
                .Where(x => x.p.SubmittedAtUtc.HasValue && x.p.ReviewedAtUtc.HasValue)
                .Select(x => (x.p.ReviewedAtUtc!.Value - x.p.SubmittedAtUtc!.Value).TotalHours)
                .DefaultIfEmpty(0)
                .AverageAsync(cancellationToken);

            var reportsReviewed = await _context.ContentReports
                .Where(cr => cr.ReviewedByUserId == stat.ModeratorId)
                .CountAsync(cancellationToken);

            var approvalRate = stat.ActionsCompleted > 0 ? 
                (double)stat.ApprovalsCount / (stat.ApprovalsCount + stat.RejectionsCount) * 100 : 0;

            performances.Add(new ModeratorPerformance
            {
                ModeratorId = stat.ModeratorId,
                ModeratorName = stat.ModeratorName,
                ActionsCompleted = stat.ActionsCompleted,
                ApprovalsCount = stat.ApprovalsCount,
                RejectionsCount = stat.RejectionsCount,
                ReportsReviewed = reportsReviewed,
                AverageReviewTimeHours = avgReviewTime,
                ApprovalRate = approvalRate,
                LastActiveAt = stat.LastActiveAt,
                ActionsLast7Days = actionsLast7Days,
                ActionsLast30Days = actionsLast30Days
            });
        }

        return performances;
    }

    public async Task<Dictionary<Domain.Enums.ModerationActionType, int>> GetActionTypeDistributionAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.ModerationActions.AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(ma => ma.CreatedAtUtc >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(ma => ma.CreatedAtUtc <= toDate.Value);

        return await query
            .GroupBy(ma => ma.ActionType)
            .ToDictionaryAsync(g => g.Key, g => g.Count(), cancellationToken);
    }

    public async Task<int> GetActionCountByModeratorAsync(Guid moderatorId, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.ModerationActions
            .Where(ma => ma.ModeratorId == moderatorId);

        if (fromDate.HasValue)
            query = query.Where(ma => ma.CreatedAtUtc >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(ma => ma.CreatedAtUtc <= toDate.Value);

        return await query.CountAsync(cancellationToken);
    }

    public async Task<double> GetAverageActionTimeByModeratorAsync(Guid moderatorId, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.ModerationActions
            .Where(ma => ma.ModeratorId == moderatorId)
            .Join(_context.Publications, ma => ma.PublicationId, p => p.Id, (ma, p) => new { ma, p })
            .Where(x => x.p.SubmittedAtUtc.HasValue && x.p.ReviewedAtUtc.HasValue);

        if (fromDate.HasValue)
            query = query.Where(x => x.ma.CreatedAtUtc >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(x => x.ma.CreatedAtUtc <= toDate.Value);

        var reviewTimes = await query
            .Select(x => (x.p.ReviewedAtUtc!.Value - x.p.SubmittedAtUtc!.Value).TotalHours)
            .ToListAsync(cancellationToken);

        return reviewTimes.Any() ? reviewTimes.Average() : 0;
    }
}
