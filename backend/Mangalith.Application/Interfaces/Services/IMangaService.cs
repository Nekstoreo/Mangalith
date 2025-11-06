using Mangalith.Domain.Entities;

namespace Mangalith.Application.Interfaces.Services;

public interface IMangaService
{
    Task<Manga?> GetMangaByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Manga>> GetPublicMangasAsync(CancellationToken cancellationToken = default);
    Task<List<Manga>> SearchPublicMangasAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<List<Manga>> GetUserMangasAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Manga> CreateMangaAsync(string title, string? description, Guid userId, CancellationToken cancellationToken = default);
    Task<Manga> UpdateMangaAsync(Guid id, string title, string? alternativeTitle, string? description, 
        string? author, string? artist, int? year, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteMangaAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> IsMangaVisibleToUserAsync(Guid mangaId, Guid? userId = null, CancellationToken cancellationToken = default);
}