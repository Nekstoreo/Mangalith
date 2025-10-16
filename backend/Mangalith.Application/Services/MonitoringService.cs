using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mangalith.Application.Common.Configuration;
using Mangalith.Application.Interfaces.Repositories;
using Mangalith.Application.Interfaces.Services;

namespace Mangalith.Application.Services;

/// <summary>
/// Service for monitoring system events and generating alerts
/// </summary>
public class MonitoringService : IMonitoringService
{
    private readonly ILogger<MonitoringService> _logger;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly PermissionSystemOptions _options;
    private readonly List<AuthorizationFailureEvent> _recentFailures = new();
    private readonly List<SystemErrorEvent> _recentErrors = new();
    private readonly List<(string Operation, TimeSpan Duration, bool Success, DateTime Timestamp)> _performanceMetrics = new();
    private readonly object _lock = new();

    public MonitoringService(
        ILogger<MonitoringService> logger,
        IAuditLogRepository auditLogRepository,
        IOptions<PermissionSystemOptions> options)
    {
        _logger = logger;
        _auditLogRepository = auditLogRepository;
        _options = options.Value;
    }

    public async Task RecordAuthorizationFailureAsync(Guid userId, string resource, string action, string reason, CancellationToken cancellationToken = default)
    {
        var failureEvent = new AuthorizationFailureEvent
        {
            UserId = userId,
            Resource = resource,
            Action = action,
            Reason = reason,
            Timestamp = DateTime.UtcNow,
            IpAddress = "Unknown", // This would be populated from HttpContext in middleware
            UserAgent = "Unknown"
        };

        lock (_lock)
        {
            _recentFailures.Add(failureEvent);
            
            // Keep only recent failures (last 1000 or last hour)
            var cutoffTime = DateTime.UtcNow.AddHours(-1);
            _recentFailures.RemoveAll(f => f.Timestamp < cutoffTime);
            
            if (_recentFailures.Count > 1000)
            {
                _recentFailures.RemoveRange(0, _recentFailures.Count - 1000);
            }
        }

        _logger.LogWarning("Authorization failure recorded: User {UserId}, Resource {Resource}, Action {Action}, Reason {Reason}",
            userId, resource, action, reason);

        // Also log to audit system if enabled
        if (_options.Audit.Enabled && _options.Audit.LogFailedOperations)
        {
            try
            {
                await _auditLogRepository.CreateAsync(new Domain.Entities.AuditLog(
                    userId, action, resource, "Unknown", false, null, reason, "Unknown"), cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log authorization failure to audit system");
            }
        }
    }

    public async Task RecordSystemErrorAsync(string component, string error, string? details = null, CancellationToken cancellationToken = default)
    {
        var errorEvent = new SystemErrorEvent
        {
            Component = component,
            Error = error,
            Details = details,
            Timestamp = DateTime.UtcNow,
            Severity = DetermineSeverity(error)
        };

        lock (_lock)
        {
            _recentErrors.Add(errorEvent);
            
            // Keep only recent errors (last 1000 or last hour)
            var cutoffTime = DateTime.UtcNow.AddHours(-1);
            _recentErrors.RemoveAll(e => e.Timestamp < cutoffTime);
            
            if (_recentErrors.Count > 1000)
            {
                _recentErrors.RemoveRange(0, _recentErrors.Count - 1000);
            }
        }

        var logLevel = errorEvent.Severity switch
        {
            "Critical" => LogLevel.Critical,
            "Error" => LogLevel.Error,
            "Warning" => LogLevel.Warning,
            _ => LogLevel.Information
        };

        _logger.Log(logLevel, "System error recorded: Component {Component}, Error {Error}, Details {Details}",
            component, error, details);

        await Task.CompletedTask;
    }

    public async Task RecordPerformanceMetricAsync(string operation, TimeSpan duration, bool success, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _performanceMetrics.Add((operation, duration, success, DateTime.UtcNow));
            
            // Keep only recent metrics (last 10000 or last hour)
            var cutoffTime = DateTime.UtcNow.AddHours(-1);
            _performanceMetrics.RemoveAll(m => m.Timestamp < cutoffTime);
            
            if (_performanceMetrics.Count > 10000)
            {
                _performanceMetrics.RemoveRange(0, _performanceMetrics.Count - 10000);
            }
        }

        if (duration.TotalSeconds > 5) // Log slow operations
        {
            _logger.LogWarning("Slow operation detected: {Operation} took {Duration}ms, Success: {Success}",
                operation, duration.TotalMilliseconds, success);
        }

        await Task.CompletedTask;
    }

    public async Task<AuthorizationFailureStatistics> GetAuthorizationFailureStatisticsAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        List<AuthorizationFailureEvent> failures;
        
        lock (_lock)
        {
            failures = _recentFailures
                .Where(f => f.Timestamp >= fromDate && f.Timestamp <= toDate)
                .ToList();
        }

        var statistics = new AuthorizationFailureStatistics
        {
            TotalFailures = failures.Count,
            FailuresByResource = failures.GroupBy(f => f.Resource).ToDictionary(g => g.Key, g => g.Count()),
            FailuresByAction = failures.GroupBy(f => f.Action).ToDictionary(g => g.Key, g => g.Count()),
            FailuresByReason = failures.GroupBy(f => f.Reason).ToDictionary(g => g.Key, g => g.Count()),
            FailuresByUser = failures.GroupBy(f => f.UserId).ToDictionary(g => g.Key, g => g.Count()),
            RecentFailures = failures.OrderByDescending(f => f.Timestamp).Take(100).ToList()
        };

        await Task.CompletedTask;
        return statistics;
    }

    public async Task<SystemErrorStatistics> GetSystemErrorStatisticsAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        List<SystemErrorEvent> errors;
        
        lock (_lock)
        {
            errors = _recentErrors
                .Where(e => e.Timestamp >= fromDate && e.Timestamp <= toDate)
                .ToList();
        }

        var statistics = new SystemErrorStatistics
        {
            TotalErrors = errors.Count,
            ErrorsByComponent = errors.GroupBy(e => e.Component).ToDictionary(g => g.Key, g => g.Count()),
            ErrorsByType = errors.GroupBy(e => e.Severity).ToDictionary(g => g.Key, g => g.Count()),
            RecentErrors = errors.OrderByDescending(e => e.Timestamp).Take(100).ToList()
        };

        await Task.CompletedTask;
        return statistics;
    }

    public async Task<PerformanceMetrics> GetPerformanceMetricsAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        List<(string Operation, TimeSpan Duration, bool Success, DateTime Timestamp)> metrics;
        
        lock (_lock)
        {
            metrics = _performanceMetrics
                .Where(m => m.Timestamp >= fromDate && m.Timestamp <= toDate)
                .ToList();
        }

        var operationGroups = metrics.GroupBy(m => m.Operation);
        var operationMetrics = new Dictionary<string, OperationMetrics>();

        foreach (var group in operationGroups)
        {
            var durations = group.Select(g => g.Duration.TotalMilliseconds).OrderBy(d => d).ToList();
            var successCount = group.Count(g => g.Success);
            var totalCount = group.Count();

            operationMetrics[group.Key] = new OperationMetrics
            {
                Operation = group.Key,
                TotalCount = totalCount,
                SuccessCount = successCount,
                FailureCount = totalCount - successCount,
                SuccessRate = totalCount > 0 ? (double)successCount / totalCount * 100 : 0,
                AverageResponseTime = durations.Count > 0 ? durations.Average() : 0,
                MinResponseTime = durations.Count > 0 ? durations.Min() : 0,
                MaxResponseTime = durations.Count > 0 ? durations.Max() : 0,
                P95ResponseTime = durations.Count > 0 ? durations[(int)(durations.Count * 0.95)] : 0
            };
        }

        var allDurations = metrics.Select(m => m.Duration.TotalMilliseconds);
        var allSuccesses = metrics.Count(m => m.Success);
        var totalOperations = metrics.Count;

        var performanceMetrics = new PerformanceMetrics
        {
            OperationMetrics = operationMetrics,
            OverallAverageResponseTime = allDurations.Any() ? allDurations.Average() : 0,
            OverallSuccessRate = totalOperations > 0 ? (double)allSuccesses / totalOperations * 100 : 0,
            TotalOperations = totalOperations
        };

        await Task.CompletedTask;
        return performanceMetrics;
    }

    public async Task<List<SystemAlert>> CheckAlertsAsync(CancellationToken cancellationToken = default)
    {
        var alerts = new List<SystemAlert>();
        var now = DateTime.UtcNow;
        var lastHour = now.AddHours(-1);

        // Check for high authorization failure rate
        var recentFailures = await GetAuthorizationFailureStatisticsAsync(lastHour, now, cancellationToken);
        if (recentFailures.TotalFailures > 100) // More than 100 failures in the last hour
        {
            alerts.Add(new SystemAlert
            {
                Type = "AuthorizationFailures",
                Severity = "Warning",
                Title = "High Authorization Failure Rate",
                Description = $"Detected {recentFailures.TotalFailures} authorization failures in the last hour",
                Data = new Dictionary<string, object>
                {
                    { "FailureCount", recentFailures.TotalFailures },
                    { "TimeWindow", "1 hour" },
                    { "TopResources", recentFailures.FailuresByResource.OrderByDescending(kvp => kvp.Value).Take(5).ToDictionary(kvp => kvp.Key, kvp => kvp.Value) }
                }
            });
        }

        // Check for system errors
        var recentErrors = await GetSystemErrorStatisticsAsync(lastHour, now, cancellationToken);
        var criticalErrors = recentErrors.RecentErrors.Count(e => e.Severity == "Critical");
        if (criticalErrors > 0)
        {
            alerts.Add(new SystemAlert
            {
                Type = "SystemErrors",
                Severity = "Critical",
                Title = "Critical System Errors Detected",
                Description = $"Detected {criticalErrors} critical system errors in the last hour",
                Data = new Dictionary<string, object>
                {
                    { "CriticalErrorCount", criticalErrors },
                    { "TotalErrorCount", recentErrors.TotalErrors },
                    { "TimeWindow", "1 hour" },
                    { "ErrorsByComponent", recentErrors.ErrorsByComponent }
                }
            });
        }

        // Check for performance issues
        var performanceMetrics = await GetPerformanceMetricsAsync(lastHour, now, cancellationToken);
        if (performanceMetrics.OverallAverageResponseTime > 2000) // More than 2 seconds average
        {
            alerts.Add(new SystemAlert
            {
                Type = "Performance",
                Severity = "Warning",
                Title = "High Response Times Detected",
                Description = $"Average response time is {performanceMetrics.OverallAverageResponseTime:F0}ms",
                Data = new Dictionary<string, object>
                {
                    { "AverageResponseTime", performanceMetrics.OverallAverageResponseTime },
                    { "SuccessRate", performanceMetrics.OverallSuccessRate },
                    { "TotalOperations", performanceMetrics.TotalOperations },
                    { "TimeWindow", "1 hour" }
                }
            });
        }

        // Check for low success rate
        if (performanceMetrics.OverallSuccessRate < 95 && performanceMetrics.TotalOperations > 10)
        {
            alerts.Add(new SystemAlert
            {
                Type = "Reliability",
                Severity = "Warning",
                Title = "Low Success Rate Detected",
                Description = $"Overall success rate is {performanceMetrics.OverallSuccessRate:F1}%",
                Data = new Dictionary<string, object>
                {
                    { "SuccessRate", performanceMetrics.OverallSuccessRate },
                    { "TotalOperations", performanceMetrics.TotalOperations },
                    { "TimeWindow", "1 hour" }
                }
            });
        }

        return alerts;
    }

    private static string DetermineSeverity(string error)
    {
        var lowerError = error.ToLowerInvariant();
        
        if (lowerError.Contains("critical") || lowerError.Contains("fatal") || lowerError.Contains("crash"))
            return "Critical";
        
        if (lowerError.Contains("error") || lowerError.Contains("exception") || lowerError.Contains("fail"))
            return "Error";
        
        if (lowerError.Contains("warning") || lowerError.Contains("warn"))
            return "Warning";
        
        return "Information";
    }
}