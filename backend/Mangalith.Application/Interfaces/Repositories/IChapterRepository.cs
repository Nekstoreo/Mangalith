using Mangalith.Domain.Entities;

namespace Mangalith.Application.Interfaces.Repositories;

public interface IChapterRepository
{
    Task<Chapter?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Chapter>> GetByMangaIdAsync(Guid mangaId, CancellationToken cancellationToken = default);
    Task<Chapter?> GetByMangaAndNumberAsync(Guid mangaId, double number, CancellationToken cancellationToken = default);
    Task AddAsync(Chapter chapter, CancellationToken cancellationToken = default);
    Task UpdateAsync(Chapter chapter, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
