namespace Mangalith.Application.Interfaces.Services;

/// <summary>
/// Service for monitoring system events and generating alerts
/// </summary>
public interface IMonitoringService
{
    /// <summary>
    /// Record an authorization failure event
    /// </summary>
    Task RecordAuthorizationFailureAsync(Guid userId, string resource, string action, string reason, CancellationToken cancellationToken = default);

    /// <summary>
    /// Record a system error event
    /// </summary>
    Task RecordSystemErrorAsync(string component, string error, string? details = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Record a performance metric
    /// </summary>
    Task RecordPerformanceMetricAsync(string operation, TimeSpan duration, bool success, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get authorization failure statistics
    /// </summary>
    Task<AuthorizationFailureStatistics> GetAuthorizationFailureStatisticsAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get system error statistics
    /// </summary>
    Task<SystemErrorStatistics> GetSystemErrorStatisticsAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get performance metrics
    /// </summary>
    Task<PerformanceMetrics> GetPerformanceMetricsAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if any alerts should be triggered
    /// </summary>
    Task<List<SystemAlert>> CheckAlertsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Authorization failure statistics
/// </summary>
public class AuthorizationFailureStatistics
{
    public int TotalFailures { get; set; }
    public Dictionary<string, int> FailuresByResource { get; set; } = new();
    public Dictionary<string, int> FailuresByAction { get; set; } = new();
    public Dictionary<string, int> FailuresByReason { get; set; } = new();
    public Dictionary<Guid, int> FailuresByUser { get; set; } = new();
    public List<AuthorizationFailureEvent> RecentFailures { get; set; } = new();
}

/// <summary>
/// System error statistics
/// </summary>
public class SystemErrorStatistics
{
    public int TotalErrors { get; set; }
    public Dictionary<string, int> ErrorsByComponent { get; set; } = new();
    public Dictionary<string, int> ErrorsByType { get; set; } = new();
    public List<SystemErrorEvent> RecentErrors { get; set; } = new();
}

/// <summary>
/// Performance metrics
/// </summary>
public class PerformanceMetrics
{
    public Dictionary<string, OperationMetrics> OperationMetrics { get; set; } = new();
    public double OverallAverageResponseTime { get; set; }
    public double OverallSuccessRate { get; set; }
    public int TotalOperations { get; set; }
}

/// <summary>
/// Metrics for a specific operation
/// </summary>
public class OperationMetrics
{
    public string Operation { get; set; } = string.Empty;
    public int TotalCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public double SuccessRate { get; set; }
    public double AverageResponseTime { get; set; }
    public double MinResponseTime { get; set; }
    public double MaxResponseTime { get; set; }
    public double P95ResponseTime { get; set; }
}

/// <summary>
/// Authorization failure event
/// </summary>
public class AuthorizationFailureEvent
{
    public Guid UserId { get; set; }
    public string Resource { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
}

/// <summary>
/// System error event
/// </summary>
public class SystemErrorEvent
{
    public string Component { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; }
    public string Severity { get; set; } = string.Empty;
}

/// <summary>
/// System alert
/// </summary>
public class SystemAlert
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Type { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Data { get; set; } = new();
    public bool IsResolved { get; set; }
    public DateTime? ResolvedAt { get; set; }
}