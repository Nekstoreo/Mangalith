namespace Mangalith.Application.Interfaces.Services;

/// <summary>
/// Service for performing health checks on system components
/// </summary>
public interface IHealthCheckService
{
    /// <summary>
    /// Check the health of the permission system
    /// </summary>
    Task<HealthCheckResult> CheckPermissionSystemHealthAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Check the health of the audit system
    /// </summary>
    Task<HealthCheckResult> CheckAuditSystemHealthAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Check the health of the quota system
    /// </summary>
    Task<HealthCheckResult> CheckQuotaSystemHealthAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Check the health of the rate limiting system
    /// </summary>
    Task<HealthCheckResult> CheckRateLimitSystemHealthAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get overall system health status
    /// </summary>
    Task<SystemHealthStatus> GetSystemHealthAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of a health check
/// </summary>
public class HealthCheckResult
{
    public bool IsHealthy { get; set; }
    public string Component { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
    public TimeSpan ResponseTime { get; set; }
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
    public string? Error { get; set; }
}

/// <summary>
/// Overall system health status
/// </summary>
public class SystemHealthStatus
{
    public bool IsHealthy { get; set; }
    public string OverallStatus { get; set; } = string.Empty;
    public List<HealthCheckResult> ComponentResults { get; set; } = new();
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
    public TimeSpan TotalResponseTime { get; set; }
    
    public Dictionary<string, object> Summary { get; set; } = new();
}