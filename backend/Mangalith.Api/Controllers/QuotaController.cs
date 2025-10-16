using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mangalith.Api.Authorization;
using Mangalith.Api.Contracts;
using Mangalith.Application.Interfaces.Services;
using Mangalith.Domain.Constants;

namespace Mangalith.Api.Controllers;

[ApiController]
[Route("api/quota")]
[Authorize]
public class QuotaController : ControllerBase
{
    private readonly IQuotaService _quotaService;
    private readonly ILogger<QuotaController> _logger;

    public QuotaController(IQuotaService quotaService, ILogger<QuotaController> logger)
    {
        _quotaService = quotaService;
        _logger = logger;
    }

    [HttpGet("usage")]
    [ProducesResponseType(typeof(QuotaUsageReport), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetQuotaUsage(CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var report = await _quotaService.GetQuotaUsageReportAsync(userId, cancellationToken);
            
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving quota usage for user");
            return StatusCode(500, new ErrorResponse(
                "internal_error", 
                "Error retrieving quota information"));
        }
    }

    [HttpGet("limits")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetQuotaLimits(CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var report = await _quotaService.GetQuotaUsageReportAsync(userId, cancellationToken);
            
            var limits = new
            {
                role = report.UserRole.ToString(),
                storage = new
                {
                    quota = report.StorageQuotaBytes,
                    used = report.StorageUsedBytes,
                    remaining = report.StorageQuotaBytes - report.StorageUsedBytes,
                    usagePercentage = report.StorageUsagePercentage,
                    isNearLimit = report.IsNearStorageLimit
                },
                uploads = new
                {
                    dailyLimit = report.DailyUploadLimit,
                    uploadedToday = report.FilesUploadedToday,
                    remaining = Math.Max(0, report.DailyUploadLimit - report.FilesUploadedToday)
                },
                manga = new
                {
                    creationLimit = report.MangaCreationLimit,
                    created = report.MangasCreated,
                    remaining = report.MangaCreationLimit == int.MaxValue ? 
                        int.MaxValue : 
                        Math.Max(0, report.MangaCreationLimit - report.MangasCreated)
                },
                api = new
                {
                    callsPerMinute = QuotaLimits.GetApiCallLimit(report.UserRole)
                },
                fileSize = new
                {
                    maxFileSize = QuotaLimits.GetMaxFileSize(report.UserRole)
                }
            };
            
            return Ok(limits);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving quota limits for user");
            return StatusCode(500, new ErrorResponse(
                "internal_error", 
                "Error retrieving quota limits"));
        }
    }

    [HttpGet("system-stats")]
    [RequirePermission(Permissions.System.Audit)]
    [ProducesResponseType(typeof(QuotaSystemStats), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSystemQuotaStats(CancellationToken cancellationToken)
    {
        try
        {
            var stats = await _quotaService.GetSystemQuotaStatsAsync(cancellationToken);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving system quota statistics");
            return StatusCode(500, new ErrorResponse(
                "internal_error", 
                "Error retrieving system statistics"));
        }
    }

    [HttpGet("users-near-limit")]
    [RequirePermission(Permissions.System.Audit)]
    [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUsersNearLimit(
        [FromQuery] double threshold = 80.0, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var users = await _quotaService.GetUsersNearLimitAsync(threshold, cancellationToken);
            
            var result = users.Select(u => new
            {
                userId = u.UserId,
                storageUsed = u.StorageUsedBytes,
                storageUsagePercentage = u.GetStorageUsagePercentage(u.User.Role),
                filesUploadedToday = u.FilesUploadedToday,
                mangasCreated = u.MangasCreated,
                lastUpdated = u.UpdatedAtUtc,
                userRole = u.User.Role.ToString(),
                userEmail = u.User.Email
            });
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users near quota limit");
            return StatusCode(500, new ErrorResponse(
                "internal_error", 
                "Error retrieving users near limit"));
        }
    }

    [HttpPost("recalculate-storage")]
    [RequirePermission(Permissions.System.Maintenance)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RecalculateStorageUsage(CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _quotaService.RecalculateUserStorageUsageAsync(userId, cancellationToken);
            
            _logger.LogInformation("Storage usage recalculated for user {UserId}", userId);
            return Ok(new { message = "Storage usage recalculated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recalculating storage usage");
            return StatusCode(500, new ErrorResponse(
                "internal_error", 
                "Error recalculating storage usage"));
        }
    }

    [HttpPost("cleanup-rate-limits")]
    [RequirePermission(Permissions.System.Maintenance)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CleanupRateLimits(CancellationToken cancellationToken)
    {
        try
        {
            await _quotaService.CleanupExpiredRateLimitEntriesAsync(cancellationToken);
            
            _logger.LogInformation("Rate limit cleanup completed");
            return Ok(new { message = "Rate limit entries cleaned up successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up rate limit entries");
            return StatusCode(500, new ErrorResponse(
                "internal_error", 
                "Error cleaning up rate limits"));
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user token");
        }
        return userId;
    }
}