using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mangalith.Application.Interfaces.Services;
using Mangalith.Domain.Enums;

namespace Mangalith.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Moderator,Administrator")]
public class ModerationController : ControllerBase
{
    private readonly IModerationService _moderationService;
    private readonly ILogger<ModerationController> _logger;

    public ModerationController(IModerationService moderationService, ILogger<ModerationController> logger)
    {
        _moderationService = moderationService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene la cola de moderación
    /// </summary>
    [HttpGet("queue")]
    public async Task<IActionResult> GetModerationQueueAsync(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] PublicationStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _moderationService.GetModerationQueueAsync(page, pageSize, status, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving moderation queue");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene el historial de moderación de una publicación
    /// </summary>
    [HttpGet("publications/{publicationId}/history")]
    public async Task<IActionResult> GetModerationHistoryAsync(
        Guid publicationId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _moderationService.GetModerationHistoryAsync(publicationId, page, pageSize, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving moderation history for publication {PublicationId}", publicationId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene las acciones de moderación de un moderador específico
    /// </summary>
    [HttpGet("moderators/{moderatorId}/actions")]
    public async Task<IActionResult> GetModeratorActionsAsync(
        Guid moderatorId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _moderationService.GetModeratorActionsAsync(moderatorId, page, pageSize, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving moderator actions for moderator {ModeratorId}", moderatorId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Realiza acciones en lote sobre múltiples publicaciones
    /// </summary>
    [HttpPost("bulk-action")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> BulkModerationActionAsync(
        [FromBody] BulkModerationActionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var moderatorId = GetUserIdFromToken();
            await _moderationService.BulkModerationActionAsync(
                request.PublicationIds,
                request.ActionType,
                moderatorId,
                request.Comments,
                cancellationToken);

            return Ok(new { message = $"Bulk action completed for {request.PublicationIds.Count()} publications" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing bulk moderation action");
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

public class BulkModerationActionRequest
{
    public IEnumerable<Guid> PublicationIds { get; set; } = new List<Guid>();
    public ModerationActionType ActionType { get; set; }
    public string Comments { get; set; } = string.Empty;
}
