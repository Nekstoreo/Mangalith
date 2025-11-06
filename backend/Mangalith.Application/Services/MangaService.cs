using Mangalith.Application.Common.Exceptions;
using Mangalith.Application.Interfaces.Repositories;
using Mangalith.Application.Interfaces.Services;
using Mangalith.Domain.Entities;
using Mangalith.Domain.Enums;

namespace Mangalith.Application.Services;

public class MangaService : IMangaService
{
    private readonly IMangaRepository _mangaRepository;
    private readonly IPublicationService _publicationService;
    private readonly IUserRepository _userRepository;

    public MangaService(
        IMangaRepository mangaRepository,
        IPublicationService publicationService,
        IUserRepository userRepository)
    {
        _mangaRepository = mangaRepository;
        _publicationService = publicationService;
        _userRepository = userRepository;
    }

    public async Task<Manga?> GetMangaByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _mangaRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<List<Manga>> GetPublicMangasAsync(CancellationToken cancellationToken = default)
    {
        return await _mangaRepository.GetPublicMangasAsync(cancellationToken);
    }

    public async Task<List<Manga>> SearchPublicMangasAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        return await _mangaRepository.SearchPublicMangasAsync(searchTerm, cancellationToken);
    }

    public async Task<List<Manga>> GetUserMangasAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _mangaRepository.GetByUserIdAsync(userId, cancellationToken);
    }

    public async Task<Manga> CreateMangaAsync(string title, string? description, Guid userId, CancellationToken cancellationToken = default)
    {
        // Verificar que el usuario existe
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException($"User with ID {userId} not found");

        // Crear manga
        var manga = new Manga(title, description, userId);
        await _mangaRepository.AddAsync(manga, cancellationToken);

        // Crear publicación automáticamente
        try
        {
            await _publicationService.CreatePublicationAsync(manga.Id, userId, cancellationToken);
        }
        catch (Exception)
        {
            // Si falla la creación de la publicación, continuar sin ella
            // El manga puede existir sin publicación para casos especiales
        }

        return manga;
    }

    public async Task<Manga> UpdateMangaAsync(Guid id, string title, string? alternativeTitle, string? description, 
        string? author, string? artist, int? year, Guid userId, CancellationToken cancellationToken = default)
    {
        var manga = await _mangaRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Manga with ID {id} not found");

        // Verificar permisos: solo el creador puede editar
        if (manga.CreatedByUserId != userId)
        {
            throw new ForbiddenAppException("You can only edit your own manga");
        }

        // Verificar si el manga está en un estado que permite edición
        if (manga.Publication != null)
        {
            var canEdit = manga.Publication.Status switch
            {
                PublicationStatus.Draft => true,
                PublicationStatus.NeedsRevision => true,
                PublicationStatus.Rejected => true,
                _ => false
            };

            if (!canEdit)
            {
                throw new InvalidOperationException($"Cannot edit manga with publication status {manga.Publication.Status}");
            }
        }

        manga.UpdateBasicInfo(title, alternativeTitle, description, author, artist, year);
        await _mangaRepository.UpdateAsync(manga, cancellationToken);

        return manga;
    }

    public async Task<bool> DeleteMangaAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var manga = await _mangaRepository.GetByIdAsync(id, cancellationToken);
        if (manga == null)
            return false;

        // Verificar permisos: solo el creador puede eliminar
        if (manga.CreatedByUserId != userId)
        {
            throw new ForbiddenAppException("You can only delete your own manga");
        }

        // Verificar si el manga puede ser eliminado (no debe estar publicado)
        if (manga.Publication?.Status == PublicationStatus.Published)
        {
            throw new InvalidOperationException("Cannot delete published manga");
        }

        await _mangaRepository.DeleteAsync(id, cancellationToken);
        return true;
    }

    public async Task<bool> IsMangaVisibleToUserAsync(Guid mangaId, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var manga = await _mangaRepository.GetByIdAsync(mangaId, cancellationToken);
        if (manga == null)
            return false;

        // Si el usuario es el creador, siempre puede ver el manga
        if (userId.HasValue && manga.CreatedByUserId == userId.Value)
            return true;

        // Si no hay publicación, solo el creador puede verlo
        if (manga.Publication == null)
            return false;

        // Para otros usuarios, solo pueden ver mangas publicados
        return manga.Publication.Status == PublicationStatus.Published;
    }
}