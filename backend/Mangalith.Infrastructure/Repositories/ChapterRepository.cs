using Microsoft.EntityFrameworkCore;
using Mangalith.Application.Interfaces.Repositories;
using Mangalith.Domain.Entities;
using Mangalith.Infrastructure.Data;

namespace Mangalith.Infrastructure.Repositories;

public class ChapterRepository : IChapterRepository
{
    private readonly MangalithDbContext _context;

    public ChapterRepository(MangalithDbContext context)
    {
        _context = context;
    }

    public async Task<Chapter?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Chapters
            .Include(c => c.Pages)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<List<Chapter>> GetByMangaIdAsync(Guid mangaId, CancellationToken cancellationToken = default)
    {
        return await _context.Chapters
            .Where(c => c.MangaId == mangaId)
            .OrderBy(c => c.Number)
            .Include(c => c.Pages)
            .ToListAsync(cancellationToken);
    }

    public async Task<Chapter?> GetByMangaAndNumberAsync(Guid mangaId, double number, CancellationToken cancellationToken = default)
    {
        return await _context.Chapters
            .FirstOrDefaultAsync(c => c.MangaId == mangaId && c.Number == number, cancellationToken);
    }

    public async Task AddAsync(Chapter chapter, CancellationToken cancellationToken = default)
    {
        await _context.Chapters.AddAsync(chapter, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Chapter chapter, CancellationToken cancellationToken = default)
    {
        _context.Chapters.Update(chapter);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var chapter = await GetByIdAsync(id, cancellationToken);
        if (chapter != null)
        {
            _context.Chapters.Remove(chapter);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
