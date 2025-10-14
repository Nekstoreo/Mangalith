using Microsoft.EntityFrameworkCore;
using Mangalith.Application.Interfaces.Repositories;
using Mangalith.Domain.Entities;
using Mangalith.Infrastructure.Data;

namespace Mangalith.Infrastructure.Repositories;

public class EfMangaFileRepository : IMangaFileRepository
{
    private readonly MangalithDbContext _context;

    public EfMangaFileRepository(MangalithDbContext context)
    {
        _context = context;
    }

    public async Task<MangaFile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.MangaFiles
            .Include(mf => mf.UploadedByUser)
            .Include(mf => mf.Manga)
            .FirstOrDefaultAsync(mf => mf.Id == id, cancellationToken);
    }

    public async Task<MangaFile?> GetByHashAsync(string fileHash, CancellationToken cancellationToken = default)
    {
        return await _context.MangaFiles
            .Include(mf => mf.UploadedByUser)
            .Include(mf => mf.Manga)
            .FirstOrDefaultAsync(mf => mf.FileHash == fileHash, cancellationToken);
    }

    public async Task<IEnumerable<MangaFile>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.MangaFiles
            .Include(mf => mf.Manga)
            .Where(mf => mf.UploadedByUserId == userId)
            .OrderByDescending(mf => mf.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<MangaFile> AddAsync(MangaFile mangaFile, CancellationToken cancellationToken = default)
    {
        _context.MangaFiles.Add(mangaFile);
        await _context.SaveChangesAsync(cancellationToken);
        return mangaFile;
    }

    public async Task UpdateAsync(MangaFile mangaFile, CancellationToken cancellationToken = default)
    {
        _context.MangaFiles.Update(mangaFile);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var mangaFile = await GetByIdAsync(id, cancellationToken);
        if (mangaFile != null)
        {
            _context.MangaFiles.Remove(mangaFile);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}