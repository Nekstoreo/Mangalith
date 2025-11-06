using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mangalith.Application.Interfaces.Services;
using Mangalith.Domain.Enums;
using Mangalith.Domain.Constants;

namespace Mangalith.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PublicationController : ControllerBase
{
    private readonly IPublicationService _publicationService;
    private readonly ILogger<PublicationController> _logger;

    public PublicationController(IPublicationService publicationService, ILogger<PublicationController> logger)
    {
        _publicationService = publicationService;
        _logger = logger;
    }

    /// <summary>
    /// Crea una nueva publicación para un manga
    /// </summary>
    [HttpPost("{mangaId}/create")]
    public async Task<IActionResult> CreatePublicationAsync(Guid mangaId, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserIdFromToken();
            var publication = await _publicationService.CreatePublicationAsync(mangaId, userId, cancellationToken);
            return CreatedAtAction(nameof(GetPublicationByIdAsync), new { publicationId = publication.Id }, new { publication.Id, publication.Status });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating publication for manga {MangaId}", mangaId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Envía una publicación para revisión
    /// </summary>
    [HttpPost("{publicationId}/submit")]
    public async Task<IActionResult> SubmitForReviewAsync(Guid publicationId, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserIdFromToken();
            var publication = await _publicationService.SubmitForReviewAsync(publicationId, userId, cancellationToken);
            return Ok(new { publication.Id, publication.Status, publication.SubmittedAtUtc });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting publication {PublicationId} for review", publicationId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Aprueba una publicación (solo para moderadores)
    /// </summary>
    [HttpPost("{publicationId}/approve")]
    [Authorize(Roles = "Moderator,Administrator")]
    public async Task<IActionResult> ApprovePublicationAsync(
        Guid publicationId,
        [FromBody] ApprovePublicationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var moderatorId = GetUserIdFromToken();
            var publication = await _publicationService.ApprovePublicationAsync(
                publicationId,
                moderatorId,
                request.ContentRating,
                request.IsNsfw,
                request.Comments,
                cancellationToken);
            
            return Ok(new { publication.Id, publication.Status, publication.ContentRating, publication.IsNsfw });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving publication {PublicationId}", publicationId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Rechaza una publicación (solo para moderadores)
    /// </summary>
    [HttpPost("{publicationId}/reject")]
    [Authorize(Roles = "Moderator,Administrator")]
    public async Task<IActionResult> RejectPublicationAsync(
        Guid publicationId,
        [FromBody] RejectPublicationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var moderatorId = GetUserIdFromToken();
            var publication = await _publicationService.RejectPublicationAsync(
                publicationId,
                moderatorId,
                request.Reason,
                request.Comments,
                cancellationToken);
            
            return Ok(new { publication.Id, publication.Status, publication.RejectionReason });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting publication {PublicationId}", publicationId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Solicita revisión de una publicación (solo para moderadores)
    /// </summary>
    [HttpPost("{publicationId}/request-revision")]
    [Authorize(Roles = "Moderator,Administrator")]
    public async Task<IActionResult> RequestRevisionAsync(
        Guid publicationId,
        [FromBody] RequestRevisionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var moderatorId = GetUserIdFromToken();
            var publication = await _publicationService.RequestRevisionAsync(
                publicationId,
                moderatorId,
                request.Comments,
                cancellationToken);
            
            return Ok(new { publication.Id, publication.Status, publication.ModeratorComments });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting revision for publication {PublicationId}", publicationId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Archiva una publicación
    /// </summary>
    [HttpPost("{publicationId}/archive")]
    public async Task<IActionResult> ArchivePublicationAsync(Guid publicationId, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserIdFromToken();
            var publication = await _publicationService.ArchivePublicationAsync(publicationId, userId, cancellationToken: cancellationToken);
            return Ok(new { publication.Id, publication.Status });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving publication {PublicationId}", publicationId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene una publicación por ID
    /// </summary>
    [HttpGet("{publicationId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicationByIdAsync(Guid publicationId, CancellationToken cancellationToken)
    {
        try
        {
            var publication = await _publicationService.GetPublicationByIdAsync(publicationId, cancellationToken);
            if (publication == null)
                return NotFound();

            return Ok(new
            {
                publication.Id,
                publication.MangaId,
                publication.Status,
                publication.ContentRating,
                publication.IsNsfw,
                publication.CreatedAtUtc,
                publication.SubmittedAtUtc,
                publication.ReviewedAtUtc
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving publication {PublicationId}", publicationId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene las publicaciones del usuario actual
    /// </summary>
    [HttpGet("my-publications")]
    public async Task<IActionResult> GetMyPublicationsAsync(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetUserIdFromToken();
            var result = await _publicationService.GetUserPublicationsAsync(userId, page, pageSize, cancellationToken);

            // Mapear a un DTO simple para evitar ciclos de serialización
            var response = new
            {
                items = result.Items.Select(p => new PublicationListItemResponse
                {
                    Id = p.Id,
                    MangaId = p.MangaId,
                    Status = p.Status.ToString(),
                    ContentRating = p.ContentRating.ToString(),
                    IsNsfw = p.IsNsfw,
                    CreatedAtUtc = p.CreatedAtUtc,
                    UpdatedAtUtc = p.UpdatedAtUtc,
                    SubmittedAtUtc = p.SubmittedAtUtc,
                    ReviewedAtUtc = p.ReviewedAtUtc,
                    Manga = p.Manga is null ? null : new PublicationListItemResponse.MangaSummary
                    {
                        Id = p.Manga.Id,
                        Title = p.Manga.Title,
                        ChapterCount = p.Manga.ChapterCount
                    }
                }),
                result.TotalCount,
                result.Page,
                result.PageSize,
                result.TotalPages,
                result.HasPreviousPage,
                result.HasNextPage
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user publications");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene publicaciones por estado
    /// </summary>
    [HttpGet("by-status/{status}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicationsByStatusAsync(
        PublicationStatus status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _publicationService.GetPublicationsByStatusAsync(status, page, pageSize, cancellationToken);

            // Mapear a un DTO simple para evitar ciclos de serialización
            var response = new
            {
                items = result.Items.Select(p => new PublicationListItemResponse
                {
                    Id = p.Id,
                    MangaId = p.MangaId,
                    Status = p.Status.ToString(),
                    ContentRating = p.ContentRating.ToString(),
                    IsNsfw = p.IsNsfw,
                    CreatedAtUtc = p.CreatedAtUtc,
                    UpdatedAtUtc = p.UpdatedAtUtc,
                    SubmittedAtUtc = p.SubmittedAtUtc,
                    ReviewedAtUtc = p.ReviewedAtUtc,
                    Manga = p.Manga is null ? null : new PublicationListItemResponse.MangaSummary
                    {
                        Id = p.Manga.Id,
                        Title = p.Manga.Title,
                        ChapterCount = p.Manga.ChapterCount
                    }
                }),
                result.TotalCount,
                result.Page,
                result.PageSize,
                result.TotalPages,
                result.HasPreviousPage,
                result.HasNextPage
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving publications by status {Status}", status);
            return BadRequest(new { error = ex.Message });
        }
    }

    private Guid GetUserIdFromToken()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            throw new UnauthorizedAccessException("Invalid or missing user ID in token");
        return userId;
    }
}

public class PublicationListItemResponse
{
    public Guid Id { get; set; }
    public Guid MangaId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ContentRating { get; set; } = string.Empty;
    public bool IsNsfw { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public DateTime? SubmittedAtUtc { get; set; }
    public DateTime? ReviewedAtUtc { get; set; }
    public MangaSummary? Manga { get; set; }

    public class MangaSummary
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int ChapterCount { get; set; }
    }
}

public class ApprovePublicationRequest
{
    public ContentRating ContentRating { get; set; }
    public bool IsNsfw { get; set; }
    public string? Comments { get; set; }
}

public class RejectPublicationRequest
{
    public string Reason { get; set; } = string.Empty;
    public string Comments { get; set; } = string.Empty;
}

public class RequestRevisionRequest
{
    public string Comments { get; set; } = string.Empty;
}
