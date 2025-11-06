using Microsoft.EntityFrameworkCore;
using Mangalith.Application.Common.Models;
using Mangalith.Application.Interfaces.Repositories;
using Mangalith.Domain.Entities;
using Mangalith.Domain.Enums;
using Mangalith.Infrastructure.Data;

namespace Mangalith.Infrastructure.Repositories;

public class PublicationRepository : IPublicationRepository
{
    private readonly MangalithDbContext _context;

    public PublicationRepository(MangalithDbContext context)
    {
        _context = context;
    }

    public async Task<Publication?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Publications
            .Include(p => p.Manga)
            .Include(p => p.CreatedByUser)
            .Include(p => p.ReviewedByUser)
            .Include(p => p.ModerationActions)
            .Include(p => p.Reports)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Publication?> GetByMangaIdAsync(Guid mangaId, CancellationToken cancellationToken = default)
    {
        return await _context.Publications
            .Include(p => p.Manga)
            .Include(p => p.CreatedByUser)
            .Include(p => p.ReviewedByUser)
            .FirstOrDefaultAsync(p => p.MangaId == mangaId, cancellationToken);
    }

    public async Task<PagedResult<Publication>> GetByStatusAsync(PublicationStatus status, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Publications
            .Where(p => p.Status == status)
            .Include(p => p.Manga)
            .Include(p => p.CreatedByUser)
            .OrderByDescending(p => p.CreatedAtUtc);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Publication>(items, total, page, pageSize);
    }

    public async Task<PagedResult<Publication>> GetByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Publications
            .Where(p => p.CreatedByUserId == userId)
            .Include(p => p.Manga)
            .Include(p => p.CreatedByUser)
            .OrderByDescending(p => p.CreatedAtUtc);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Publication>(items, total, page, pageSize);
    }

    public async Task<Publication> CreateAsync(Publication publication, CancellationToken cancellationToken = default)
    {
        await _context.Publications.AddAsync(publication, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return publication;
    }

    public async Task<Publication> UpdateAsync(Publication publication, CancellationToken cancellationToken = default)
    {
        _context.Publications.Update(publication);
        await _context.SaveChangesAsync(cancellationToken);
        return publication;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var publication = await GetByIdAsync(id, cancellationToken);
        if (publication != null)
        {
            _context.Publications.Remove(publication);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<int> GetCountByStatusAsync(PublicationStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Publications
            .Where(p => p.Status == status)
            .CountAsync(cancellationToken);
    }

    public async Task<PagedResult<Publication>> GetModerationQueueAsync(int page, int pageSize, PublicationStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Publications
            .Include(p => p.Manga)
            .Include(p => p.CreatedByUser)
            .Include(p => p.Reports)
            .AsQueryable();

        // Si status es nulo, incluir InReview, NeedsRevision y UnderReview
        if (status == null)
        {
            query = query.Where(p => p.Status == PublicationStatus.InReview || 
                                      p.Status == PublicationStatus.NeedsRevision || 
                                      p.Status == PublicationStatus.UnderReview);
        }
        else
        {
            query = query.Where(p => p.Status == status);
        }

        // Ordenar por: urgencia (UnderReview primero), luego por fecha de envío
        query = query.OrderBy(p => p.Status != PublicationStatus.UnderReview)
                     .ThenBy(p => p.SubmittedAtUtc);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Publication>(items, total, page, pageSize);
    }

    public async Task<Dictionary<PublicationStatus, int>> GetStatusDistributionAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Publications.AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(p => p.CreatedAtUtc >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(p => p.CreatedAtUtc <= toDate.Value);

        return await query
            .GroupBy(p => p.Status)
            .ToDictionaryAsync(g => g.Key, g => g.Count(), cancellationToken);
    }

    public async Task<Dictionary<ContentRating, int>> GetContentRatingDistributionAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Publications.AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(p => p.CreatedAtUtc >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(p => p.CreatedAtUtc <= toDate.Value);

        return await query
            .GroupBy(p => p.ContentRating)
            .ToDictionaryAsync(g => g.Key, g => g.Count(), cancellationToken);
    }

    public async Task<List<PublicationTrend>> GetPublicationTrendsAsync(int days = 30, CancellationToken cancellationToken = default)
    {
        var startDate = DateTime.UtcNow.AddDays(-days).Date;
        var endDate = DateTime.UtcNow.Date.AddDays(1);

        var publications = await _context.Publications
            .Where(p => p.CreatedAtUtc >= startDate && p.CreatedAtUtc < endDate)
            .Select(p => new { p.CreatedAtUtc, p.Status, p.SubmittedAtUtc, p.ReviewedAtUtc })
            .ToListAsync(cancellationToken);

        var trends = new List<PublicationTrend>();
        
        for (var date = startDate; date < endDate; date = date.AddDays(1))
        {
            var dayPublications = publications.Where(p => p.CreatedAtUtc.Date == date).ToList();
            
            var trend = new PublicationTrend
            {
                Date = date,
                Submissions = dayPublications.Count,
                Approvals = dayPublications.Count(p => p.Status == PublicationStatus.Published),
                Rejections = dayPublications.Count(p => p.Status == PublicationStatus.Rejected),
                AverageReviewTime = dayPublications
                    .Where(p => p.SubmittedAtUtc.HasValue && p.ReviewedAtUtc.HasValue)
                    .Select(p => (p.ReviewedAtUtc!.Value - p.SubmittedAtUtc!.Value).TotalHours)
                    .DefaultIfEmpty(0)
                    .Average()
            };
            
            trends.Add(trend);
        }

        return trends;
    }

    public async Task<double> GetAverageReviewTimeAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Publications
            .Where(p => p.SubmittedAtUtc.HasValue && p.ReviewedAtUtc.HasValue);

        if (fromDate.HasValue)
            query = query.Where(p => p.SubmittedAtUtc >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(p => p.ReviewedAtUtc <= toDate.Value);

        var reviewTimes = await query
            .Select(p => (p.ReviewedAtUtc!.Value - p.SubmittedAtUtc!.Value).TotalHours)
            .ToListAsync(cancellationToken);

        return reviewTimes.Any() ? reviewTimes.Average() : 0;
    }

    public async Task<Dictionary<string, int>> GetTopCreatorsAsync(int limit = 10, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Publications
            .Include(p => p.CreatedByUser)
            .AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(p => p.CreatedAtUtc >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(p => p.CreatedAtUtc <= toDate.Value);

        return await query
            .GroupBy(p => p.CreatedByUser.FullName)
            .OrderByDescending(g => g.Count())
            .Take(limit)
            .ToDictionaryAsync(g => g.Key, g => g.Count(), cancellationToken);
    }

    public async Task<int> GetSubmissionCountAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Publications.AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(p => p.CreatedAtUtc >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(p => p.CreatedAtUtc <= toDate.Value);

        return await query.CountAsync(cancellationToken);
    }
}
