using Microsoft.AspNetCore.Mvc;
using Mangalith.Application.Interfaces.Services;
using Mangalith.Api.Authorization;

namespace Mangalith.Api.Controllers;

/// <summary>
/// Controller for system health checks and monitoring
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IHealthCheckService _healthCheckService;
    private readonly IMonitoringService _monitoringService;
    private readonly ILogger<HealthController> _logger;

    public HealthController(
        IHealthCheckService healthCheckService,
        IMonitoringService monitoringService,
        ILogger<HealthController> logger)
    {
        _healthCheckService = healthCheckService;
        _monitoringService = monitoringService;
        _logger = logger;
    }

    /// <summary>
    /// Get basic health status
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetHealth(CancellationToken cancellationToken = default)
    {
        try
        {
            var health = await _healthCheckService.GetSystemHealthAsync(cancellationToken);
            
            var statusCode = health.OverallStatus switch
            {
                "Healthy" => 200,
                "Degraded" => 200,
                "Unhealthy" => 503,
                "Critical" => 503,
                _ => 503
            };

            return StatusCode(statusCode, new
            {
                status = health.OverallStatus,
                timestamp = health.CheckedAt,
                responseTime = health.TotalResponseTime.TotalMilliseconds,
                components = health.ComponentResults.Select(c => new
                {
                    name = c.Component,
                    status = c.Status,
                    responseTime = c.ResponseTime.TotalMilliseconds
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return StatusCode(503, new
            {
                status = "Critical",
                timestamp = DateTime.UtcNow,
                error = "Health check failed"
            });
        }
    }

    /// <summary>
    /// Get detailed health status (requires admin access)
    /// </summary>
    [HttpGet("detailed")]
    [RequirePermission("system.monitor")]
    public async Task<IActionResult> GetDetailedHealth(CancellationToken cancellationToken = default)
    {
        try
        {
            var health = await _healthCheckService.GetSystemHealthAsync(cancellationToken);
            return Ok(health);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Detailed health check failed");
            return StatusCode(500, new { error = "Detailed health check failed" });
        }
    }

    /// <summary>
    /// Get permission system health
    /// </summary>
    [HttpGet("permissions")]
    [RequirePermission("system.monitor")]
    public async Task<IActionResult> GetPermissionSystemHealth(CancellationToken cancellationToken = default)
    {
        try
        {
            var health = await _healthCheckService.CheckPermissionSystemHealthAsync(cancellationToken);
            return Ok(health);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Permission system health check failed");
            return StatusCode(500, new { error = "Permission system health check failed" });
        }
    }

    /// <summary>
    /// Get audit system health
    /// </summary>
    [HttpGet("audit")]
    [RequirePermission("system.monitor")]
    public async Task<IActionResult> GetAuditSystemHealth(CancellationToken cancellationToken = default)
    {
        try
        {
            var health = await _healthCheckService.CheckAuditSystemHealthAsync(cancellationToken);
            return Ok(health);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Audit system health check failed");
            return StatusCode(500, new { error = "Audit system health check failed" });
        }
    }

    /// <summary>
    /// Get quota system health
    /// </summary>
    [HttpGet("quotas")]
    [RequirePermission("system.monitor")]
    public async Task<IActionResult> GetQuotaSystemHealth(CancellationToken cancellationToken = default)
    {
        try
        {
            var health = await _healthCheckService.CheckQuotaSystemHealthAsync(cancellationToken);
            return Ok(health);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Quota system health check failed");
            return StatusCode(500, new { error = "Quota system health check failed" });
        }
    }

    /// <summary>
    /// Get rate limiting system health
    /// </summary>
    [HttpGet("ratelimit")]
    [RequirePermission("system.monitor")]
    public async Task<IActionResult> GetRateLimitSystemHealth(CancellationToken cancellationToken = default)
    {
        try
        {
            var health = await _healthCheckService.CheckRateLimitSystemHealthAsync(cancellationToken);
            return Ok(health);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rate limit system health check failed");
            return StatusCode(500, new { error = "Rate limit system health check failed" });
        }
    }

    /// <summary>
    /// Get system alerts
    /// </summary>
    [HttpGet("alerts")]
    [RequirePermission("system.monitor")]
    public async Task<IActionResult> GetAlerts(CancellationToken cancellationToken = default)
    {
        try
        {
            var alerts = await _monitoringService.CheckAlertsAsync(cancellationToken);
            return Ok(alerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get system alerts");
            return StatusCode(500, new { error = "Failed to get system alerts" });
        }
    }

    /// <summary>
    /// Get authorization failure statistics
    /// </summary>
    [HttpGet("monitoring/auth-failures")]
    [RequirePermission("system.monitor")]
    public async Task<IActionResult> GetAuthorizationFailures(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var from = fromDate ?? DateTime.UtcNow.AddHours(-24);
            var to = toDate ?? DateTime.UtcNow;

            var statistics = await _monitoringService.GetAuthorizationFailureStatisticsAsync(from, to, cancellationToken);
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get authorization failure statistics");
            return StatusCode(500, new { error = "Failed to get authorization failure statistics" });
        }
    }

    /// <summary>
    /// Get system error statistics
    /// </summary>
    [HttpGet("monitoring/system-errors")]
    [RequirePermission("system.monitor")]
    public async Task<IActionResult> GetSystemErrors(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var from = fromDate ?? DateTime.UtcNow.AddHours(-24);
            var to = toDate ?? DateTime.UtcNow;

            var statistics = await _monitoringService.GetSystemErrorStatisticsAsync(from, to, cancellationToken);
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get system error statistics");
            return StatusCode(500, new { error = "Failed to get system error statistics" });
        }
    }

    /// <summary>
    /// Get performance metrics
    /// </summary>
    [HttpGet("monitoring/performance")]
    [RequirePermission("system.monitor")]
    public async Task<IActionResult> GetPerformanceMetrics(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var from = fromDate ?? DateTime.UtcNow.AddHours(-24);
            var to = toDate ?? DateTime.UtcNow;

            var metrics = await _monitoringService.GetPerformanceMetricsAsync(from, to, cancellationToken);
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get performance metrics");
            return StatusCode(500, new { error = "Failed to get performance metrics" });
        }
    }
}