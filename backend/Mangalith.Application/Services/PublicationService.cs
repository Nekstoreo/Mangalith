using Mangalith.Application.Common.Exceptions;
using Mangalith.Application.Common.Models;
using Mangalith.Application.Interfaces.Repositories;
using Mangalith.Application.Interfaces.Services;
using Mangalith.Domain.Entities;
using Mangalith.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Mangalith.Application.Services;

public class PublicationService : IPublicationService
{
    private readonly IPublicationRepository _publicationRepository;
    private readonly IMangaRepository _mangaRepository;
    private readonly IUserRepository _userRepository;
    private readonly IChapterRepository _chapterRepository;
    private readonly IPublicationValidationService _validationService;
    private readonly ILogger<PublicationService> _logger;

    public PublicationService(
        IPublicationRepository publicationRepository,
        IMangaRepository mangaRepository,
        IUserRepository userRepository,
        IChapterRepository chapterRepository,
        IPublicationValidationService validationService,
        ILogger<PublicationService> logger)
    {
        _publicationRepository = publicationRepository;
        _mangaRepository = mangaRepository;
        _userRepository = userRepository;
        _chapterRepository = chapterRepository;
        _validationService = validationService;
        _logger = logger;
    }

    public async Task<Publication> CreatePublicationAsync(Guid mangaId, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating publication for manga {MangaId} by user {UserId}", mangaId, userId);

        // Validate publication creation
        await _validationService.ValidatePublicationCreationAsync(mangaId, userId, cancellationToken);

        // Verificar que no exista ya una publicación para este manga
        var existingPublication = await _publicationRepository.GetByMangaIdAsync(mangaId, cancellationToken);
        if (existingPublication != null)
        {
            throw new ConflictAppException("A publication already exists for this manga");
        }

        // Crear nueva publicación
        var publication = new Publication(mangaId, userId);
        var createdPublication = await _publicationRepository.CreateAsync(publication, cancellationToken);
        
        _logger.LogInformation("Publication {PublicationId} created successfully for manga {MangaId}", 
            createdPublication.Id, mangaId);
        
        return createdPublication;
    }

    public async Task<Publication> SubmitForReviewAsync(Guid publicationId, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Submitting publication {PublicationId} for review by user {UserId}", publicationId, userId);

        var publication = await _publicationRepository.GetByIdAsync(publicationId, cancellationToken)
            ?? throw new NotFoundException($"Publication with ID {publicationId} not found");

        // Verificar que el usuario es el creador
        if (publication.CreatedByUserId != userId)
        {
            throw new ForbiddenAppException("You can only submit your own publications");
        }

        // Validate submission requirements
        await _validationService.ValidatePublicationSubmissionAsync(publication, cancellationToken);

        publication.SubmitForReview();
        var updatedPublication = await _publicationRepository.UpdateAsync(publication, cancellationToken);
        
        _logger.LogInformation("Publication {PublicationId} submitted for review successfully", publicationId);
        
        return updatedPublication;
    }

    public async Task<Publication> ApprovePublicationAsync(Guid publicationId, Guid moderatorId, ContentRating rating, bool isNsfw, string? comments = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Approving publication {PublicationId} by moderator {ModeratorId}", publicationId, moderatorId);

        var publication = await _publicationRepository.GetByIdAsync(publicationId, cancellationToken)
            ?? throw new NotFoundException($"Publication with ID {publicationId} not found");

        // Validate moderator permissions
        await _validationService.ValidateModeratorPermissionsAsync(moderatorId, cancellationToken);

        // Validate moderation action
        _validationService.ValidateModerationAction(publication.Status, PublicationStatus.Published, comments);

        // Validate content rating
        if (!Enum.IsDefined(typeof(ContentRating), rating))
        {
            throw new ModerationValidationException("INVALID_CONTENT_RATING", "Invalid content rating specified");
        }

        publication.Approve(moderatorId, rating, isNsfw, comments);
        var updatedPublication = await _publicationRepository.UpdateAsync(publication, cancellationToken);
        
        _logger.LogInformation("Publication {PublicationId} approved successfully with rating {Rating}, NSFW: {IsNsfw}", 
            publicationId, rating, isNsfw);
        
        return updatedPublication;
    }

    public async Task<Publication> RejectPublicationAsync(Guid publicationId, Guid moderatorId, string reason, string comments, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Rejecting publication {PublicationId} by moderator {ModeratorId}", publicationId, moderatorId);

        var publication = await _publicationRepository.GetByIdAsync(publicationId, cancellationToken)
            ?? throw new NotFoundException($"Publication with ID {publicationId} not found");

        // Validate moderator permissions
        await _validationService.ValidateModeratorPermissionsAsync(moderatorId, cancellationToken);

        // Validate moderation action
        _validationService.ValidateModerationAction(publication.Status, PublicationStatus.Rejected, comments, reason);

        publication.Reject(moderatorId, reason, comments);
        var updatedPublication = await _publicationRepository.UpdateAsync(publication, cancellationToken);
        
        _logger.LogWarning("Publication {PublicationId} rejected. Reason: {Reason}", publicationId, reason);
        
        return updatedPublication;
    }

    public async Task<Publication> RequestRevisionAsync(Guid publicationId, Guid moderatorId, string comments, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Requesting revision for publication {PublicationId} by moderator {ModeratorId}", publicationId, moderatorId);

        var publication = await _publicationRepository.GetByIdAsync(publicationId, cancellationToken)
            ?? throw new NotFoundException($"Publication with ID {publicationId} not found");

        // Validate moderator permissions
        await _validationService.ValidateModeratorPermissionsAsync(moderatorId, cancellationToken);

        // Validate moderation action
        _validationService.ValidateModerationAction(publication.Status, PublicationStatus.NeedsRevision, comments);

        publication.RequestRevision(moderatorId, comments);
        var updatedPublication = await _publicationRepository.UpdateAsync(publication, cancellationToken);
        
        _logger.LogInformation("Revision requested for publication {PublicationId}", publicationId);
        
        return updatedPublication;
    }

    public async Task<Publication> ArchivePublicationAsync(Guid publicationId, Guid userId, string? reason = null, CancellationToken cancellationToken = default)
    {
        var publication = await _publicationRepository.GetByIdAsync(publicationId, cancellationToken)
            ?? throw new NotFoundException($"Publication with ID {publicationId} not found");

        // Verificar permisos: el creador o un admin pueden archivar
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException($"User with ID {userId} not found");

        bool isCreator = publication.CreatedByUserId == userId;
        bool isAdmin = user.Role == UserRole.Administrator;

        if (!isCreator && !isAdmin)
        {
            throw new ForbiddenAppException("Only the creator or an administrator can archive this publication");
        }

        publication.Archive();
        return await _publicationRepository.UpdateAsync(publication, cancellationToken);
    }

    public async Task<PagedResult<Publication>> GetPublicationsByStatusAsync(PublicationStatus status, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _publicationRepository.GetByStatusAsync(status, page, pageSize, cancellationToken);
    }

    public async Task<PagedResult<Publication>> GetUserPublicationsAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _publicationRepository.GetByUserIdAsync(userId, page, pageSize, cancellationToken);
    }

    public async Task<Publication?> GetPublicationByIdAsync(Guid publicationId, CancellationToken cancellationToken = default)
    {
        return await _publicationRepository.GetByIdAsync(publicationId, cancellationToken);
    }

    public async Task<Publication?> GetPublicationByMangaIdAsync(Guid mangaId, CancellationToken cancellationToken = default)
    {
        return await _publicationRepository.GetByMangaIdAsync(mangaId, cancellationToken);
    }
}
