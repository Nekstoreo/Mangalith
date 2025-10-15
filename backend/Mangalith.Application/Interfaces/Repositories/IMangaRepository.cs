using Mangalith.Domain.Entities;

namespace Mangalith.Application.Interfaces.Repositories;

public interface IMangaRepository
{
    Task<Manga?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Manga>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<Manga>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Manga?> GetByTitleAsync(string title, CancellationToken cancellationToken = default);
    Task AddAsync(Manga manga, CancellationToken cancellationToken = default);
    Task UpdateAsync(Manga manga, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
