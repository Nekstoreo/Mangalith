using Mangalith.Domain.Entities;

namespace Mangalith.Application.Interfaces.Services;

public interface IQuotaService
{
    // Storage quota methods
    Task<bool> CanUploadFileAsync(Guid userId, long fileSize, CancellationToken cancellationToken = default);
    Task<bool> CheckStorageQuotaAsync(Guid userId, long additionalBytes, CancellationToken cancellationToken = default);
    Task TrackFileUploadAsync(Guid userId, long fileSize, CancellationToken cancellationToken = default);
    Task TrackFileDeleteAsync(Guid userId, long fileSize, CancellationToken cancellationToken = default);
    Task<UserQuota> GetUserQuotaAsync(Guid userId, CancellationToken cancellationToken = default);
    
    // Manga creation quota methods
    Task<bool> CanCreateMangaAsync(Guid userId, CancellationToken cancellationToken = default);
    Task TrackMangaCreationAsync(Guid userId, CancellationToken cancellationToken = default);
    Task TrackMangaDeletionAsync(Guid userId, CancellationToken cancellationToken = default);
    
    // Rate limiting methods
    Task<bool> CheckRateLimitAsync(Guid userId, string endpoint, CancellationToken cancellationToken = default);
    Task TrackApiCallAsync(Guid userId, string endpoint, CancellationToken cancellationToken = default);
    
    // Quota reporting methods
    Task<QuotaUsageReport> GetQuotaUsageReportAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserQuota>> GetUsersNearLimitAsync(double thresholdPercentage = 80.0, CancellationToken cancellationToken = default);
    Task<QuotaSystemStats> GetSystemQuotaStatsAsync(CancellationToken cancellationToken = default);
    
    // Maintenance methods
    Task CleanupExpiredRateLimitEntriesAsync(CancellationToken cancellationToken = default);
    Task RecalculateUserStorageUsageAsync(Guid userId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Reporte de uso de cuotas para un usuario específico
/// </summary>
public class QuotaUsageReport
{
    public Guid UserId { get; set; }
    public UserRole UserRole { get; set; }
    public long StorageUsedBytes { get; set; }
    public long StorageQuotaBytes { get; set; }
    public double StorageUsagePercentage { get; set; }
    public int FilesUploadedToday { get; set; }
    public int DailyUploadLimit { get; set; }
    public int MangasCreated { get; set; }
    public int MangaCreationLimit { get; set; }
    public bool IsNearStorageLimit { get; set; }
    public bool HasExceededAnyLimit { get; set; }
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Estadísticas del sistema de cuotas
/// </summary>
public class QuotaSystemStats
{
    public long TotalStorageUsed { get; set; }
    public int TotalUsers { get; set; }
    public int UsersNearLimit { get; set; }
    public int UsersExceedingLimit { get; set; }
    public Dictionary<UserRole, long> StorageByRole { get; set; } = new();
    public Dictionary<UserRole, int> UserCountByRole { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
}