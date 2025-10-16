namespace Mangalith.Application.Common.Configuration;

/// <summary>
/// Configuration options for the permission system
/// </summary>
public class PermissionSystemOptions
{
    public const string SectionName = "PermissionSystem";

    /// <summary>
    /// Permission caching configuration
    /// </summary>
    public PermissionCacheOptions Cache { get; set; } = new();

    /// <summary>
    /// Audit logging configuration
    /// </summary>
    public AuditOptions Audit { get; set; } = new();

    /// <summary>
    /// User quota configuration
    /// </summary>
    public QuotaOptions Quotas { get; set; } = new();

    /// <summary>
    /// Rate limiting configuration
    /// </summary>
    public RateLimitOptions RateLimit { get; set; } = new();

    /// <summary>
    /// Security configuration
    /// </summary>
    public SecurityOptions Security { get; set; } = new();
}

/// <summary>
/// Permission caching configuration
/// </summary>
public class PermissionCacheOptions
{
    /// <summary>
    /// Enable permission caching
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Cache expiration time for role-based permissions in minutes
    /// </summary>
    public int RolePermissionExpirationMinutes { get; set; } = 15;

    /// <summary>
    /// Cache expiration time for user-specific permissions in minutes
    /// </summary>
    public int UserPermissionExpirationMinutes { get; set; } = 5;

    /// <summary>
    /// Maximum number of cached permission entries
    /// </summary>
    public int MaxCacheSize { get; set; } = 10000;

    /// <summary>
    /// Enable cache statistics collection
    /// </summary>
    public bool EnableStatistics { get; set; } = true;
}

/// <summary>
/// Audit logging configuration
/// </summary>
public class AuditOptions
{
    /// <summary>
    /// Enable audit logging
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Log successful operations
    /// </summary>
    public bool LogSuccessfulOperations { get; set; } = true;

    /// <summary>
    /// Log failed operations
    /// </summary>
    public bool LogFailedOperations { get; set; } = true;

    /// <summary>
    /// Audit log retention period in days
    /// </summary>
    public int RetentionDays { get; set; } = 90;

    /// <summary>
    /// Enable automatic cleanup of old audit logs
    /// </summary>
    public bool EnableAutoCleanup { get; set; } = true;

    /// <summary>
    /// Cleanup interval in hours
    /// </summary>
    public int CleanupIntervalHours { get; set; } = 24;

    /// <summary>
    /// Maximum audit log entries to keep per cleanup
    /// </summary>
    public int MaxEntriesPerCleanup { get; set; } = 100000;

    /// <summary>
    /// Actions to exclude from audit logging
    /// </summary>
    public string[] ExcludedActions { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Resources to exclude from audit logging
    /// </summary>
    public string[] ExcludedResources { get; set; } = Array.Empty<string>();
}

/// <summary>
/// User quota configuration
/// </summary>
public class QuotaOptions
{
    /// <summary>
    /// Enable quota enforcement
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Storage quotas by role in bytes
    /// </summary>
    public Dictionary<string, long> StorageQuotasByRole { get; set; } = new()
    {
        { "Reader", 0 }, // No upload for readers
        { "Uploader", 1073741824 }, // 1GB
        { "Moderator", 5368709120 }, // 5GB
        { "Administrator", -1 } // Unlimited
    };

    /// <summary>
    /// Daily file upload limits by role
    /// </summary>
    public Dictionary<string, int> DailyUploadLimitsByRole { get; set; } = new()
    {
        { "Reader", 0 },
        { "Uploader", 10 },
        { "Moderator", 50 },
        { "Administrator", -1 } // Unlimited
    };

    /// <summary>
    /// Maximum manga creation limits by role
    /// </summary>
    public Dictionary<string, int> MangaCreationLimitsByRole { get; set; } = new()
    {
        { "Reader", 0 },
        { "Uploader", 5 },
        { "Moderator", 25 },
        { "Administrator", -1 } // Unlimited
    };

    /// <summary>
    /// Enable quota warnings
    /// </summary>
    public bool EnableWarnings { get; set; } = true;

    /// <summary>
    /// Warning threshold percentage (0-100)
    /// </summary>
    public int WarningThresholdPercent { get; set; } = 80;

    /// <summary>
    /// Enable temporary quota overrides
    /// </summary>
    public bool EnableTemporaryOverrides { get; set; } = true;

    /// <summary>
    /// Maximum temporary override duration in hours
    /// </summary>
    public int MaxOverrideDurationHours { get; set; } = 168; // 1 week
}

/// <summary>
/// Rate limiting configuration
/// </summary>
public class RateLimitOptions
{
    /// <summary>
    /// Enable role-based rate limiting
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Rate limits by role (requests per minute)
    /// </summary>
    public Dictionary<string, int> LimitsByRole { get; set; } = new()
    {
        { "Reader", 60 },
        { "Uploader", 120 },
        { "Moderator", 300 },
        { "Administrator", 600 }
    };

    /// <summary>
    /// Rate limit window in minutes
    /// </summary>
    public int WindowMinutes { get; set; } = 1;

    /// <summary>
    /// Enable burst allowance
    /// </summary>
    public bool EnableBurst { get; set; } = true;

    /// <summary>
    /// Burst multiplier (e.g., 2.0 allows double the rate for short bursts)
    /// </summary>
    public double BurstMultiplier { get; set; } = 1.5;

    /// <summary>
    /// Endpoints to exclude from rate limiting
    /// </summary>
    public string[] ExcludedEndpoints { get; set; } = new[]
    {
        "/health",
        "/metrics",
        "/swagger"
    };
}

/// <summary>
/// Security configuration
/// </summary>
public class SecurityOptions
{
    /// <summary>
    /// Enable permission validation on every request
    /// </summary>
    public bool EnableStrictValidation { get; set; } = true;

    /// <summary>
    /// Enable session management
    /// </summary>
    public bool EnableSessionManagement { get; set; } = true;

    /// <summary>
    /// Maximum concurrent sessions per user
    /// </summary>
    public int MaxConcurrentSessions { get; set; } = 5;

    /// <summary>
    /// Enable IP address validation
    /// </summary>
    public bool EnableIpValidation { get; set; } = false;

    /// <summary>
    /// Enable user agent validation
    /// </summary>
    public bool EnableUserAgentValidation { get; set; } = false;

    /// <summary>
    /// Token refresh threshold in minutes (refresh when token has less than this time left)
    /// </summary>
    public int TokenRefreshThresholdMinutes { get; set; } = 15;

    /// <summary>
    /// Enable automatic token refresh
    /// </summary>
    public bool EnableAutoTokenRefresh { get; set; } = true;

    /// <summary>
    /// Failed login attempt threshold before account lockout
    /// </summary>
    public int FailedLoginThreshold { get; set; } = 5;

    /// <summary>
    /// Account lockout duration in minutes
    /// </summary>
    public int LockoutDurationMinutes { get; set; } = 30;

    /// <summary>
    /// Enable account lockout
    /// </summary>
    public bool EnableAccountLockout { get; set; } = true;
}