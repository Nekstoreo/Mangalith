using Microsoft.EntityFrameworkCore;
using Mangalith.Application.Interfaces.Repositories;
using Mangalith.Domain.Entities;
using Mangalith.Infrastructure.Data;

namespace Mangalith.Infrastructure.Repositories;

public class MangaRepository : IMangaRepository
{
    private readonly MangalithDbContext _context;

    public MangaRepository(MangalithDbContext context)
    {
        _context = context;
    }

    public async Task<Manga?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Mangas
            .Include(m => m.Chapters)
            .Include(m => m.Files)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<List<Manga>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Mangas
            .Include(m => m.Chapters)
            .OrderByDescending(m => m.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Manga>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Mangas
            .Where(m => m.CreatedByUserId == userId)
            .Include(m => m.Chapters)
            .OrderByDescending(m => m.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<Manga?> GetByTitleAsync(string title, CancellationToken cancellationToken = default)
    {
        return await _context.Mangas
            .FirstOrDefaultAsync(m => m.Title == title, cancellationToken);
    }

    public async Task AddAsync(Manga manga, CancellationToken cancellationToken = default)
    {
        await _context.Mangas.AddAsync(manga, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Manga manga, CancellationToken cancellationToken = default)
    {
        _context.Mangas.Update(manga);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var manga = await GetByIdAsync(id, cancellationToken);
        if (manga != null)
        {
            _context.Mangas.Remove(manga);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
