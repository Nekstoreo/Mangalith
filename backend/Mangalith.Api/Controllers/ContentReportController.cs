using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mangalith.Application.Interfaces.Services;
using Mangalith.Domain.Enums;

namespace Mangalith.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ContentReportController : ControllerBase
{
    private readonly IContentReportService _contentReportService;
    private readonly ILogger<ContentReportController> _logger;

    public ContentReportController(IContentReportService contentReportService, ILogger<ContentReportController> logger)
    {
        _contentReportService = contentReportService;
        _logger = logger;
    }

    /// <summary>
    /// Crea un nuevo reporte de contenido
    /// </summary>
    [HttpPost("create")]
    public async Task<IActionResult> CreateReportAsync(
        [FromBody] CreateContentReportRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserIdFromToken();
            var report = await _contentReportService.CreateReportAsync(
                request.PublicationId,
                userId,
                request.Category,
                request.Description,
                cancellationToken);

            return CreatedAtAction(nameof(GetReportByIdAsync), new { reportId = report.Id }, new { report.Id, report.Status });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating content report for publication {PublicationId}", request.PublicationId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene un reporte por ID
    /// </summary>
    [HttpGet("{reportId}")]
    public async Task<IActionResult> GetReportByIdAsync(Guid reportId, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserIdFromToken();
            // En una implementación más completa, verificarías permisos aquí
            
            // Por ahora, solo retornamos un placeholder
            return Ok(new { reportId, message = "Report retrieved successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving report {ReportId}", reportId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene los reportes pendientes (solo para moderadores)
    /// </summary>
    [HttpGet("pending")]
    [Authorize(Roles = "Moderator,Administrator")]
    public async Task<IActionResult> GetPendingReportsAsync(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _contentReportService.GetPendingReportsAsync(page, pageSize, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending reports");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene reportes por publicación
    /// </summary>
    [HttpGet("publications/{publicationId}")]
    [Authorize(Roles = "Moderator,Administrator")]
    public async Task<IActionResult> GetReportsByPublicationAsync(
        Guid publicationId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _contentReportService.GetReportsByPublicationAsync(publicationId, page, pageSize, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reports for publication {PublicationId}", publicationId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene reportes del usuario actual
    /// </summary>
    [HttpGet("my-reports")]
    public async Task<IActionResult> GetMyReportsAsync(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetUserIdFromToken();
            var result = await _contentReportService.GetUserReportsAsync(userId, page, pageSize, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user reports");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Revisa un reporte (solo para moderadores)
    /// </summary>
    [HttpPost("{reportId}/review")]
    [Authorize(Roles = "Moderator,Administrator")]
    public async Task<IActionResult> ReviewReportAsync(
        Guid reportId,
        [FromBody] ReviewReportRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var moderatorId = GetUserIdFromToken();
            var report = await _contentReportService.ReviewReportAsync(
                reportId,
                moderatorId,
                request.Status,
                request.Response,
                cancellationToken);

            return Ok(new { report.Id, report.Status, report.ModeratorResponse });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reviewing report {ReportId}", reportId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene el conteo de reportes por publicación
    /// </summary>
    [HttpGet("publications/{publicationId}/count")]
    [Authorize(Roles = "Moderator,Administrator")]
    public async Task<IActionResult> GetReportCountByPublicationAsync(
        Guid publicationId,
        CancellationToken cancellationToken)
    {
        try
        {
            var count = await _contentReportService.GetReportCountByPublicationAsync(publicationId, cancellationToken);
            return Ok(new { publicationId, reportCount = count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting report count for publication {PublicationId}", publicationId);
            return BadRequest(new { error = ex.Message });
        }
    }

    private Guid GetUserIdFromToken()
    {
        var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("nameid");
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            throw new UnauthorizedAccessException("Invalid or missing user ID in token");
        return userId;
    }
}

public class CreateContentReportRequest
{
    public Guid PublicationId { get; set; }
    public ContentReportCategory Category { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class ReviewReportRequest
{
    public ContentReportStatus Status { get; set; }
    public string? Response { get; set; }
}
