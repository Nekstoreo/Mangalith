namespace Mangalith.Application.Common.Exceptions;

/// <summary>
/// Excepción lanzada cuando se excede una cuota de usuario
/// </summary>
public class QuotaExceededException : AppException
{
    public string QuotaType { get; }
    public long CurrentUsage { get; }
    public long Limit { get; }

    public QuotaExceededException(string quotaType, long currentUsage, long limit)
        : base("quota_exceeded", $"{quotaType} quota exceeded. Current usage: {FormatBytes(currentUsage)}, Limit: {FormatBytes(limit)}")
    {
        QuotaType = quotaType;
        CurrentUsage = currentUsage;
        Limit = limit;
    }

    public QuotaExceededException(string quotaType, int currentUsage, int limit)
        : base("quota_exceeded", $"{quotaType} quota exceeded. Current usage: {currentUsage}, Limit: {limit}")
    {
        QuotaType = quotaType;
        CurrentUsage = currentUsage;
        Limit = limit;
    }

    private static string FormatBytes(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024.0 * 1024):F1} MB";
        return $"{bytes / (1024.0 * 1024 * 1024):F1} GB";
    }
}

/// <summary>
/// Excepción lanzada cuando se excede el límite de rate limiting
/// </summary>
public class RateLimitExceededException : AppException
{
    public int RequestCount { get; }
    public int Limit { get; }
    public TimeSpan RetryAfter { get; }

    public RateLimitExceededException(int requestCount, int limit, TimeSpan retryAfter)
        : base("rate_limit_exceeded", $"Rate limit exceeded. {requestCount}/{limit} requests made. Retry after {retryAfter.TotalSeconds:F0} seconds.")
    {
        RequestCount = requestCount;
        Limit = limit;
        RetryAfter = retryAfter;
    }
}