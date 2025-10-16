using Microsoft.EntityFrameworkCore;
using Mangalith.Application.Interfaces.Repositories;
using Mangalith.Domain.Entities;
using Mangalith.Infrastructure.Data;

namespace Mangalith.Infrastructure.Repositories;

public class RateLimitRepository : IRateLimitRepository
{
    private readonly MangalithDbContext _context;

    public RateLimitRepository(MangalithDbContext context)
    {
        _context = context;
    }

    public async Task<RateLimitEntry?> GetByUserAndEndpointAsync(Guid userId, string endpoint, CancellationToken cancellationToken = default)
    {
        return await _context.RateLimitEntries
            .Include(rle => rle.User)
            .FirstOrDefaultAsync(rle => rle.UserId == userId && rle.Endpoint == endpoint, cancellationToken);
    }

    public async Task<RateLimitEntry> CreateAsync(RateLimitEntry rateLimitEntry, CancellationToken cancellationToken = default)
    {
        _context.RateLimitEntries.Add(rateLimitEntry);
        await _context.SaveChangesAsync(cancellationToken);
        return rateLimitEntry;
    }

    public async Task<RateLimitEntry> UpdateAsync(RateLimitEntry rateLimitEntry, CancellationToken cancellationToken = default)
    {
        _context.RateLimitEntries.Update(rateLimitEntry);
        await _context.SaveChangesAsync(cancellationToken);
        return rateLimitEntry;
    }

    public async Task DeleteExpiredEntriesAsync(CancellationToken cancellationToken = default)
    {
        var cutoffTime = DateTime.UtcNow.AddMinutes(-5); // Eliminar entradas más antiguas de 5 minutos
        
        var expiredEntries = await _context.RateLimitEntries
            .Where(rle => rle.WindowStartUtc < cutoffTime)
            .ToListAsync(cancellationToken);

        if (expiredEntries.Any())
        {
            _context.RateLimitEntries.RemoveRange(expiredEntries);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IEnumerable<RateLimitEntry>> GetActiveEntriesForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var cutoffTime = DateTime.UtcNow.AddMinutes(-1); // Entradas activas de la última hora
        
        return await _context.RateLimitEntries
            .Include(rle => rle.User)
            .Where(rle => rle.UserId == userId && rle.WindowStartUtc >= cutoffTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<string, int>> GetEndpointUsageStatsAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        var stats = await _context.RateLimitEntries
            .Where(rle => rle.WindowStartUtc >= fromDate && rle.WindowStartUtc <= toDate)
            .GroupBy(rle => rle.Endpoint)
            .Select(g => new { Endpoint = g.Key, TotalRequests = g.Sum(rle => rle.RequestCount) })
            .ToListAsync(cancellationToken);

        return stats.ToDictionary(s => s.Endpoint, s => s.TotalRequests);
    }

    public async Task<IEnumerable<RateLimitEntry>> GetRecentEntriesAsync(DateTime fromDate, CancellationToken cancellationToken = default)
    {
        return await _context.RateLimitEntries
            .Include(rle => rle.User)
            .Where(rle => rle.WindowStartUtc >= fromDate)
            .OrderByDescending(rle => rle.WindowStartUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.RateLimitEntries.CountAsync(cancellationToken);
    }
}