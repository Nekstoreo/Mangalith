using Mangalith.Application.Common.Models;
using Mangalith.Domain.Entities;
using Mangalith.Domain.Enums;

namespace Mangalith.Application.Interfaces.Services;

public interface IPublicationService
{
    Task<Publication> CreatePublicationAsync(Guid mangaId, Guid userId, CancellationToken cancellationToken = default);
    Task<Publication> SubmitForReviewAsync(Guid publicationId, Guid userId, CancellationToken cancellationToken = default);
    Task<Publication> ApprovePublicationAsync(Guid publicationId, Guid moderatorId, ContentRating rating, bool isNsfw, string? comments = null, CancellationToken cancellationToken = default);
    Task<Publication> RejectPublicationAsync(Guid publicationId, Guid moderatorId, string reason, string comments, CancellationToken cancellationToken = default);
    Task<Publication> RequestRevisionAsync(Guid publicationId, Guid moderatorId, string comments, CancellationToken cancellationToken = default);
    Task<Publication> ArchivePublicationAsync(Guid publicationId, Guid userId, string? reason = null, CancellationToken cancellationToken = default);
    Task<PagedResult<Publication>> GetPublicationsByStatusAsync(PublicationStatus status, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PagedResult<Publication>> GetUserPublicationsAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Publication?> GetPublicationByIdAsync(Guid publicationId, CancellationToken cancellationToken = default);
    Task<Publication?> GetPublicationByMangaIdAsync(Guid mangaId, CancellationToken cancellationToken = default);
}
