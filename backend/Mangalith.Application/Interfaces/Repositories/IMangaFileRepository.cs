using Mangalith.Domain.Entities;

namespace Mangalith.Application.Interfaces.Repositories;

public interface IMangaFileRepository
{
    Task<MangaFile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<MangaFile?> GetByHashAsync(string fileHash, CancellationToken cancellationToken = default);
    Task<IEnumerable<MangaFile>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<MangaFile> AddAsync(MangaFile mangaFile, CancellationToken cancellationToken = default);
    Task UpdateAsync(MangaFile mangaFile, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}