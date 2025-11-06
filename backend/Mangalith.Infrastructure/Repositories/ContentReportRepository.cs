using Microsoft.EntityFrameworkCore;
using Mangalith.Application.Common.Models;
using Mangalith.Application.Interfaces.Repositories;
using Mangalith.Domain.Entities;
using Mangalith.Domain.Enums;
using Mangalith.Infrastructure.Data;

namespace Mangalith.Infrastructure.Repositories;

public class ContentReportRepository : IContentReportRepository
{
    private readonly MangalithDbContext _context;

    public ContentReportRepository(MangalithDbContext context)
    {
        _context = context;
    }

    public async Task<ContentReport?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ContentReports
            .Include(cr => cr.Publication)
            .Include(cr => cr.ReportedByUser)
            .Include(cr => cr.ReviewedByUser)
            .FirstOrDefaultAsync(cr => cr.Id == id, cancellationToken);
    }

    public async Task<PagedResult<ContentReport>> GetByPublicationIdAsync(Guid publicationId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.ContentReports
            .Where(cr => cr.PublicationId == publicationId)
            .Include(cr => cr.ReportedByUser)
            .Include(cr => cr.ReviewedByUser)
            .OrderByDescending(cr => cr.CreatedAtUtc);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<ContentReport>(items, total, page, pageSize);
    }

    public async Task<PagedResult<ContentReport>> GetByStatusAsync(ContentReportStatus status, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.ContentReports
            .Where(cr => cr.Status == status)
            .Include(cr => cr.Publication)
            .Include(cr => cr.ReportedByUser)
            .OrderByDescending(cr => cr.CreatedAtUtc);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<ContentReport>(items, total, page, pageSize);
    }

    public async Task<PagedResult<ContentReport>> GetPendingAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await GetByStatusAsync(ContentReportStatus.Pending, page, pageSize, cancellationToken);
    }

    public async Task<ContentReport> CreateAsync(ContentReport report, CancellationToken cancellationToken = default)
    {
        await _context.ContentReports.AddAsync(report, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return report;
    }

    public async Task<ContentReport> UpdateAsync(ContentReport report, CancellationToken cancellationToken = default)
    {
        _context.ContentReports.Update(report);
        await _context.SaveChangesAsync(cancellationToken);
        return report;
    }

    public async Task<PagedResult<ContentReport>> GetByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.ContentReports
            .Where(cr => cr.ReportedByUserId == userId)
            .Include(cr => cr.Publication)
            .Include(cr => cr.ReviewedByUser)
            .OrderByDescending(cr => cr.CreatedAtUtc);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<ContentReport>(items, total, page, pageSize);
    }

    public async Task<IEnumerable<ContentReport>> GetByPublicationAndUserAsync(Guid publicationId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.ContentReports
            .Where(cr => cr.PublicationId == publicationId && cr.ReportedByUserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountByPublicationIdAsync(Guid publicationId, CancellationToken cancellationToken = default)
    {
        return await _context.ContentReports
            .Where(cr => cr.PublicationId == publicationId)
            .CountAsync(cancellationToken);
    }

    public async Task<int> GetCountByStatusAsync(ContentReportStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.ContentReports
            .Where(cr => cr.Status == status)
            .CountAsync(cancellationToken);
    }

    public async Task<Dictionary<ContentReportCategory, int>> GetCategoryDistributionAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.ContentReports.AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(cr => cr.CreatedAtUtc >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(cr => cr.CreatedAtUtc <= toDate.Value);

        return await query
            .GroupBy(cr => cr.Category)
            .ToDictionaryAsync(g => g.Key, g => g.Count(), cancellationToken);
    }

    public async Task<int> GetReportCountAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.ContentReports.AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(cr => cr.CreatedAtUtc >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(cr => cr.CreatedAtUtc <= toDate.Value);

        return await query.CountAsync(cancellationToken);
    }
}
