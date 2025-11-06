using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mangalith.Application.Interfaces.Services;
using Mangalith.Api.Contracts;

namespace Mangalith.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MangaController : ControllerBase
{
    private readonly IMangaService _mangaService;
    private readonly IPublicationService _publicationService;
    private readonly ILogger<MangaController> _logger;

    public MangaController(
        IMangaService mangaService,
        IPublicationService publicationService,
        ILogger<MangaController> logger)
    {
        _mangaService = mangaService;
        _publicationService = publicationService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los mangas públicos (solo publicados)
    /// </summary>
    [HttpGet("public")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicMangasAsync(CancellationToken cancellationToken)
    {
        try
        {
            var mangas = await _mangaService.GetPublicMangasAsync(cancellationToken);
            var response = mangas.Select(m => new MangaResponse
            {
                Id = m.Id,
                Title = m.Title,
                AlternativeTitle = m.AlternativeTitle,
                Description = m.Description,
                Author = m.Author,
                Artist = m.Artist,
                Year = m.Year,
                Status = m.Status.ToString(),
                CoverImagePath = m.CoverImagePath,
                Tags = m.Tags,
                Genres = m.Genres,
                ChapterCount = m.ChapterCount,
                ViewCount = m.ViewCount,
                Rating = m.Rating,
                RatingCount = m.RatingCount,
                CreatedAtUtc = m.CreatedAtUtc,
                UpdatedAtUtc = m.UpdatedAtUtc,
                PublicationStatus = m.Publication?.Status.ToString(),
                ContentRating = m.Publication?.ContentRating.ToString(),
                IsNsfw = m.Publication?.IsNsfw ?? false
            });

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving public mangas");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Busca mangas públicos por término de búsqueda
    /// </summary>
    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<IActionResult> SearchPublicMangasAsync(
        [FromQuery] string? q,
        CancellationToken cancellationToken)
    {
        try
        {
            var mangas = await _mangaService.SearchPublicMangasAsync(q ?? string.Empty, cancellationToken);
            var response = mangas.Select(m => new MangaResponse
            {
                Id = m.Id,
                Title = m.Title,
                AlternativeTitle = m.AlternativeTitle,
                Description = m.Description,
                Author = m.Author,
                Artist = m.Artist,
                Year = m.Year,
                Status = m.Status.ToString(),
                CoverImagePath = m.CoverImagePath,
                Tags = m.Tags,
                Genres = m.Genres,
                ChapterCount = m.ChapterCount,
                ViewCount = m.ViewCount,
                Rating = m.Rating,
                RatingCount = m.RatingCount,
                CreatedAtUtc = m.CreatedAtUtc,
                UpdatedAtUtc = m.UpdatedAtUtc,
                PublicationStatus = m.Publication?.Status.ToString(),
                ContentRating = m.Publication?.ContentRating.ToString(),
                IsNsfw = m.Publication?.IsNsfw ?? false
            });

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching mangas with term: {SearchTerm}", q);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene un manga por ID (respeta visibilidad basada en publicación)
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetMangaByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserIdFromTokenOrNull();
            var isVisible = await _mangaService.IsMangaVisibleToUserAsync(id, userId, cancellationToken);
            
            if (!isVisible)
            {
                return NotFound();
            }

            var manga = await _mangaService.GetMangaByIdAsync(id, cancellationToken);
            if (manga == null)
            {
                return NotFound();
            }

            var response = new MangaDetailResponse
            {
                Id = manga.Id,
                Title = manga.Title,
                AlternativeTitle = manga.AlternativeTitle,
                Description = manga.Description,
                Author = manga.Author,
                Artist = manga.Artist,
                Year = manga.Year,
                Status = manga.Status.ToString(),
                CoverImagePath = manga.CoverImagePath,
                Tags = manga.Tags,
                Genres = manga.Genres,
                ChapterCount = manga.ChapterCount,
                ViewCount = manga.ViewCount,
                Rating = manga.Rating,
                RatingCount = manga.RatingCount,
                CreatedAtUtc = manga.CreatedAtUtc,
                UpdatedAtUtc = manga.UpdatedAtUtc,
                CreatedByUserId = manga.CreatedByUserId,
                Chapters = manga.Chapters.Select(c => new ChapterResponse
                {
                    Id = c.Id,
                    Title = c.Title,
                    Number = c.Number,
                    VolumeNumber = c.VolumeNumber,
                    PageCount = c.PageCount,
                    CreatedAtUtc = c.CreatedAtUtc
                }).ToList(),
                Publication = manga.Publication != null ? new PublicationResponse
                {
                    Id = manga.Publication.Id,
                    Status = manga.Publication.Status.ToString(),
                    ContentRating = manga.Publication.ContentRating.ToString(),
                    IsNsfw = manga.Publication.IsNsfw,
                    ModeratorComments = manga.Publication.ModeratorComments,
                    RejectionReason = manga.Publication.RejectionReason,
                    SubmittedAtUtc = manga.Publication.SubmittedAtUtc,
                    ReviewedAtUtc = manga.Publication.ReviewedAtUtc,
                    CreatedAtUtc = manga.Publication.CreatedAtUtc
                } : null
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving manga {MangaId}", id);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene los mangas del usuario actual
    /// </summary>
    [HttpGet("my-mangas")]
    [Authorize]
    public async Task<IActionResult> GetMyMangasAsync(CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserIdFromToken();
            var mangas = await _mangaService.GetUserMangasAsync(userId, cancellationToken);
            
            var response = mangas.Select(m => new MangaResponse
            {
                Id = m.Id,
                Title = m.Title,
                AlternativeTitle = m.AlternativeTitle,
                Description = m.Description,
                Author = m.Author,
                Artist = m.Artist,
                Year = m.Year,
                Status = m.Status.ToString(),
                CoverImagePath = m.CoverImagePath,
                Tags = m.Tags,
                Genres = m.Genres,
                ChapterCount = m.ChapterCount,
                ViewCount = m.ViewCount,
                Rating = m.Rating,
                RatingCount = m.RatingCount,
                CreatedAtUtc = m.CreatedAtUtc,
                UpdatedAtUtc = m.UpdatedAtUtc,
                PublicationStatus = m.Publication?.Status.ToString(),
                ContentRating = m.Publication?.ContentRating.ToString(),
                IsNsfw = m.Publication?.IsNsfw ?? false
            });

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user mangas");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Crea un nuevo manga
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateMangaAsync(
        [FromBody] CreateMangaRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserIdFromToken();
            var manga = await _mangaService.CreateMangaAsync(request.Title, request.Description, userId, cancellationToken);
            
            return CreatedAtAction(
                nameof(GetMangaByIdAsync),
                new { id = manga.Id },
                new { manga.Id, manga.Title, manga.Status });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating manga");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Actualiza un manga existente
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateMangaAsync(
        Guid id,
        [FromBody] UpdateMangaRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserIdFromToken();
            var manga = await _mangaService.UpdateMangaAsync(
                id, request.Title, request.AlternativeTitle, request.Description,
                request.Author, request.Artist, request.Year, userId, cancellationToken);
            
            return Ok(new { manga.Id, manga.Title, manga.UpdatedAtUtc });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating manga {MangaId}", id);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Elimina un manga
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteMangaAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserIdFromToken();
            var deleted = await _mangaService.DeleteMangaAsync(id, userId, cancellationToken);
            
            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting manga {MangaId}", id);
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

    private Guid? GetUserIdFromTokenOrNull()
    {
        var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("nameid");
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            return null;
        return userId;
    }
}

public class CreateMangaRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateMangaRequest
{
    public string Title { get; set; } = string.Empty;
    public string? AlternativeTitle { get; set; }
    public string? Description { get; set; }
    public string? Author { get; set; }
    public string? Artist { get; set; }
    public int? Year { get; set; }
}

public class MangaResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? AlternativeTitle { get; set; }
    public string? Description { get; set; }
    public string? Author { get; set; }
    public string? Artist { get; set; }
    public int? Year { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? CoverImagePath { get; set; }
    public string? Tags { get; set; }
    public string? Genres { get; set; }
    public int ChapterCount { get; set; }
    public int ViewCount { get; set; }
    public double Rating { get; set; }
    public int RatingCount { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public string? PublicationStatus { get; set; }
    public string? ContentRating { get; set; }
    public bool IsNsfw { get; set; }
}

public class MangaDetailResponse : MangaResponse
{
    public Guid CreatedByUserId { get; set; }
    public List<ChapterResponse> Chapters { get; set; } = new();
    public PublicationResponse? Publication { get; set; }
}

public class ChapterResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public double Number { get; set; }
    public int? VolumeNumber { get; set; }
    public int PageCount { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class PublicationResponse
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ContentRating { get; set; } = string.Empty;
    public bool IsNsfw { get; set; }
    public string? ModeratorComments { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? SubmittedAtUtc { get; set; }
    public DateTime? ReviewedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}