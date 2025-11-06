using Microsoft.AspNetCore.Mvc;
using Mangalith.Api.Authorization;
using Mangalith.Application.Interfaces.Services;
using Mangalith.Application.Common.Models;
using Mangalith.Domain.Constants;
using Mangalith.Domain.Entities;
using Mangalith.Domain.Enums;

namespace Mangalith.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[RequireRole(UserRole.Administrator)]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    /// <summary>
    /// Get comprehensive moderation analytics
    /// </summary>
    [HttpGet("moderation")]
    [RequirePermission(Permissions.Moderation.ViewStatistics)]
    public async Task<ActionResult<ModerationAnalytics>> GetModerationAnalytics(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var analytics = await _analyticsService.GetModerationAnalyticsAsync(fromDate, toDate, cancellationToken);
        return Ok(analytics);
    }

    /// <summary>
    /// Get publication metrics and insights
    /// </summary>
    [HttpGet("publications")]
    [RequirePermission(Permissions.Publication.ViewAll)]
    public async Task<ActionResult<PublicationMetrics>> GetPublicationMetrics(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var metrics = await _analyticsService.GetPublicationMetricsAsync(fromDate, toDate, cancellationToken);
        return Ok(metrics);
    }

    /// <summary>
    /// Get moderator performance statistics
    /// </summary>
    [HttpGet("moderators/performance")]
    [RequirePermission(Permissions.Moderation.ViewStatistics)]
    public async Task<ActionResult<List<ModeratorPerformance>>> GetModeratorPerformance(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var performance = await _analyticsService.GetModeratorPerformanceAsync(fromDate, toDate, cancellationToken);
        return Ok(performance);
    }

    /// <summary>
    /// Get content trends analysis
    /// </summary>
    [HttpGet("trends")]
    [RequirePermission(Permissions.Moderation.ViewStatistics)]
    public async Task<ActionResult<List<ContentTrend>>> GetContentTrends(
        [FromQuery] int days = 30,
        CancellationToken cancellationToken = default)
    {
        var trends = await _analyticsService.GetContentTrendsAsync(days, cancellationToken);
        return Ok(trends);
    }

    /// <summary>
    /// Get system alerts and notifications
    /// </summary>
    [HttpGet("alerts")]
    [RequirePermission(Permissions.Moderation.ViewStatistics)]
    public async Task<ActionResult<List<Domain.Entities.SystemAlert>>> GetSystemAlerts(
        [FromQuery] bool includeResolved = false,
        CancellationToken cancellationToken = default)
    {
        var alerts = await _analyticsService.GetSystemAlertsAsync(includeResolved, cancellationToken);
        return Ok(alerts);
    }

    /// <summary>
    /// Export analytics report in specified format
    /// </summary>
    [HttpGet("export")]
    [RequirePermission(Permissions.Moderation.ViewStatistics)]
    public async Task<IActionResult> ExportAnalyticsReport(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromQuery] string format = "csv",
        CancellationToken cancellationToken = default)
    {
        var reportData = await _analyticsService.ExportAnalyticsReportAsync(fromDate, toDate, format, cancellationToken);
        
        var fileName = $"analytics-report-{fromDate:yyyy-MM-dd}-to-{toDate:yyyy-MM-dd}.{format}";
        var contentType = format.ToLower() switch
        {
            "csv" => "text/csv",
            _ => "application/octet-stream"
        };

        return File(reportData, contentType, fileName);
    }

    /// <summary>
    /// Trigger system alert check
    /// </summary>
    [HttpPost("alerts/check")]
    [RequirePermission(Permissions.Moderation.ViewStatistics)]
    public async Task<IActionResult> CheckSystemAlerts(CancellationToken cancellationToken = default)
    {
        await _analyticsService.CheckAndCreateAlertsAsync(cancellationToken);
        return Ok(new { message = "Alert check completed" });
    }
}