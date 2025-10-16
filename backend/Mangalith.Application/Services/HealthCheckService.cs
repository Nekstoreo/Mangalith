using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mangalith.Application.Common.Configuration;
using Mangalith.Application.Interfaces.Repositories;
using Mangalith.Application.Interfaces.Services;

namespace Mangalith.Application.Services;

/// <summary>
/// Service for performing health checks on system components
/// </summary>
public class HealthCheckService : IHealthCheckService
{
    private readonly ILogger<HealthCheckService> _logger;
    private readonly IPermissionService _permissionService;
    private readonly IAuditService _auditService;
    private readonly IUserRepository _userRepository;
    private readonly IUserQuotaRepository _userQuotaRepository;
    private readonly IRateLimitRepository _rateLimitRepository;
    private readonly PermissionSystemOptions _options;

    public HealthCheckService(
        ILogger<HealthCheckService> logger,
        IPermissionService permissionService,
        IAuditService auditService,
        IUserRepository userRepository,
        IUserQuotaRepository userQuotaRepository,
        IRateLimitRepository rateLimitRepository,
        IOptions<PermissionSystemOptions> options)
    {
        _logger = logger;
        _permissionService = permissionService;
        _auditService = auditService;
        _userRepository = userRepository;
        _userQuotaRepository = userQuotaRepository;
        _rateLimitRepository = rateLimitRepository;
        _options = options.Value;
    }

    public async Task<HealthCheckResult> CheckPermissionSystemHealthAsync(CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new HealthCheckResult
        {
            Component = "PermissionSystem"
        };

        try
        {
            // Test basic permission checking
            var testUserId = Guid.NewGuid();
            var hasPermission = await _permissionService.HasPermissionAsync(testUserId, "test.permission", cancellationToken);
            
            // Check if permissions are loaded
            var adminPermissions = await _permissionService.GetRolePermissionsAsync(Domain.Entities.UserRole.Administrator, cancellationToken);
            var permissionCount = adminPermissions.Count();

            result.IsHealthy = permissionCount > 0;
            result.Status = result.IsHealthy ? "Healthy" : "Unhealthy";
            result.Description = result.IsHealthy 
                ? $"Permission system is operational with {permissionCount} permissions loaded"
                : "Permission system has no permissions loaded";

            result.Data = new Dictionary<string, object>
            {
                { "PermissionCount", permissionCount },
                { "CacheEnabled", _options.Cache.Enabled },
                { "CacheStatistics", _options.Cache.EnableStatistics }
            };

            _logger.LogDebug("Permission system health check completed: {Status}", result.Status);
        }
        catch (Exception ex)
        {
            result.IsHealthy = false;
            result.Status = "Unhealthy";
            result.Description = "Permission system check failed";
            result.Error = ex.Message;
            
            _logger.LogError(ex, "Permission system health check failed");
        }
        finally
        {
            stopwatch.Stop();
            result.ResponseTime = stopwatch.Elapsed;
        }

        return result;
    }

    public async Task<HealthCheckResult> CheckAuditSystemHealthAsync(CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new HealthCheckResult
        {
            Component = "AuditSystem"
        };

        try
        {
            // Test audit log creation and retrieval
            var testUserId = Guid.NewGuid();
            await _auditService.LogActionAsync(testUserId, "health.check", "system", "127.0.0.1", success: true, cancellationToken: cancellationToken);

            // Get recent audit statistics
            var fromDate = DateTime.UtcNow.AddDays(-1);
            var toDate = DateTime.UtcNow;
            var statistics = await _auditService.GetAuditStatisticsAsync(fromDate, toDate, cancellationToken);

            result.IsHealthy = true;
            result.Status = "Healthy";
            result.Description = "Audit system is operational";

            result.Data = new Dictionary<string, object>
            {
                { "AuditEnabled", _options.Audit.Enabled },
                { "RetentionDays", _options.Audit.RetentionDays },
                { "AutoCleanupEnabled", _options.Audit.EnableAutoCleanup },
                { "RecentLogCount", statistics.TotalLogs }
            };

            _logger.LogDebug("Audit system health check completed: {Status}", result.Status);
        }
        catch (Exception ex)
        {
            result.IsHealthy = false;
            result.Status = "Unhealthy";
            result.Description = "Audit system check failed";
            result.Error = ex.Message;
            
            _logger.LogError(ex, "Audit system health check failed");
        }
        finally
        {
            stopwatch.Stop();
            result.ResponseTime = stopwatch.Elapsed;
        }

        return result;
    }

    public async Task<HealthCheckResult> CheckQuotaSystemHealthAsync(CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new HealthCheckResult
        {
            Component = "QuotaSystem"
        };

        try
        {
            // Check quota repository connectivity
            var userCount = await _userRepository.CountAsync(cancellationToken);
            var quotaCount = await _userQuotaRepository.CountAsync(cancellationToken);

            var quotaCoverage = userCount > 0 ? (double)quotaCount / userCount * 100 : 0;

            result.IsHealthy = quotaCoverage >= 90; // At least 90% of users should have quota records
            result.Status = result.IsHealthy ? "Healthy" : "Degraded";
            result.Description = result.IsHealthy 
                ? "Quota system is operational"
                : $"Quota coverage is low: {quotaCoverage:F1}%";

            result.Data = new Dictionary<string, object>
            {
                { "QuotaEnabled", _options.Quotas.Enabled },
                { "UserCount", userCount },
                { "QuotaRecordCount", quotaCount },
                { "QuotaCoveragePercent", Math.Round(quotaCoverage, 1) },
                { "WarningsEnabled", _options.Quotas.EnableWarnings }
            };

            _logger.LogDebug("Quota system health check completed: {Status}", result.Status);
        }
        catch (Exception ex)
        {
            result.IsHealthy = false;
            result.Status = "Unhealthy";
            result.Description = "Quota system check failed";
            result.Error = ex.Message;
            
            _logger.LogError(ex, "Quota system health check failed");
        }
        finally
        {
            stopwatch.Stop();
            result.ResponseTime = stopwatch.Elapsed;
        }

        return result;
    }

    public async Task<HealthCheckResult> CheckRateLimitSystemHealthAsync(CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new HealthCheckResult
        {
            Component = "RateLimitSystem"
        };

        try
        {
            // Check rate limit repository connectivity and recent activity
            var recentEntries = await _rateLimitRepository.GetRecentEntriesAsync(DateTime.UtcNow.AddHours(-1), cancellationToken);
            var entryCount = recentEntries.Count();

            result.IsHealthy = true; // Rate limiting is operational if we can query it
            result.Status = "Healthy";
            result.Description = "Rate limiting system is operational";

            result.Data = new Dictionary<string, object>
            {
                { "RateLimitEnabled", _options.RateLimit.Enabled },
                { "WindowMinutes", _options.RateLimit.WindowMinutes },
                { "BurstEnabled", _options.RateLimit.EnableBurst },
                { "RecentEntriesCount", entryCount }
            };

            _logger.LogDebug("Rate limit system health check completed: {Status}", result.Status);
        }
        catch (Exception ex)
        {
            result.IsHealthy = false;
            result.Status = "Unhealthy";
            result.Description = "Rate limit system check failed";
            result.Error = ex.Message;
            
            _logger.LogError(ex, "Rate limit system health check failed");
        }
        finally
        {
            stopwatch.Stop();
            result.ResponseTime = stopwatch.Elapsed;
        }

        return result;
    }

    public async Task<SystemHealthStatus> GetSystemHealthAsync(CancellationToken cancellationToken = default)
    {
        var overallStopwatch = Stopwatch.StartNew();
        var systemHealth = new SystemHealthStatus();

        try
        {
            // Run all health checks in parallel
            var healthCheckTasks = new[]
            {
                CheckPermissionSystemHealthAsync(cancellationToken),
                CheckAuditSystemHealthAsync(cancellationToken),
                CheckQuotaSystemHealthAsync(cancellationToken),
                CheckRateLimitSystemHealthAsync(cancellationToken)
            };

            var results = await Task.WhenAll(healthCheckTasks);
            systemHealth.ComponentResults.AddRange(results);

            // Determine overall health
            var healthyCount = results.Count(r => r.IsHealthy);
            var totalCount = results.Length;
            var healthPercentage = (double)healthyCount / totalCount * 100;

            systemHealth.IsHealthy = healthyCount == totalCount;
            systemHealth.OverallStatus = healthPercentage switch
            {
                100 => "Healthy",
                >= 75 => "Degraded",
                >= 50 => "Unhealthy",
                _ => "Critical"
            };

            // Build summary
            systemHealth.Summary = new Dictionary<string, object>
            {
                { "HealthyComponents", healthyCount },
                { "TotalComponents", totalCount },
                { "HealthPercentage", Math.Round(healthPercentage, 1) },
                { "ComponentStatus", results.ToDictionary(r => r.Component, r => r.Status) }
            };

            _logger.LogInformation("System health check completed: {Status} ({HealthyCount}/{TotalCount} components healthy)", 
                systemHealth.OverallStatus, healthyCount, totalCount);
        }
        catch (Exception ex)
        {
            systemHealth.IsHealthy = false;
            systemHealth.OverallStatus = "Critical";
            systemHealth.Summary["Error"] = ex.Message;
            
            _logger.LogError(ex, "System health check failed");
        }
        finally
        {
            overallStopwatch.Stop();
            systemHealth.TotalResponseTime = overallStopwatch.Elapsed;
        }

        return systemHealth;
    }
}