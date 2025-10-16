namespace Mangalith.Domain.Entities;

/// <summary>
/// Entidad para rastrear el uso de cuotas por usuario
/// </summary>
public class UserQuota
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public long StorageUsedBytes { get; private set; }
    public int FilesUploadedToday { get; private set; }
    public int MangasCreated { get; private set; }
    public DateTime LastResetDate { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private UserQuota()
    {
        Id = Guid.Empty;
        UserId = Guid.Empty;
        StorageUsedBytes = 0;
        FilesUploadedToday = 0;
        MangasCreated = 0;
        LastResetDate = DateTime.UtcNow.Date;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public UserQuota(Guid userId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        StorageUsedBytes = 0;
        FilesUploadedToday = 0;
        MangasCreated = 0;
        LastResetDate = DateTime.UtcNow.Date;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Incrementa el uso de almacenamiento
    /// </summary>
    public void AddStorageUsage(long bytes)
    {
        if (bytes < 0)
            throw new ArgumentException("Storage bytes cannot be negative", nameof(bytes));

        StorageUsedBytes += bytes;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Decrementa el uso de almacenamiento
    /// </summary>
    public void RemoveStorageUsage(long bytes)
    {
        if (bytes < 0)
            throw new ArgumentException("Storage bytes cannot be negative", nameof(bytes));

        StorageUsedBytes = Math.Max(0, StorageUsedBytes - bytes);
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Incrementa el contador de archivos subidos hoy
    /// </summary>
    public void IncrementFileUpload()
    {
        ResetDailyCountersIfNeeded();
        FilesUploadedToday++;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Incrementa el contador de mangas creados
    /// </summary>
    public void IncrementMangaCreation()
    {
        MangasCreated++;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Decrementa el contador de mangas creados
    /// </summary>
    public void DecrementMangaCreation()
    {
        MangasCreated = Math.Max(0, MangasCreated - 1);
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Resetea los contadores diarios si es necesario
    /// </summary>
    public void ResetDailyCountersIfNeeded()
    {
        var today = DateTime.UtcNow.Date;
        if (LastResetDate < today)
        {
            FilesUploadedToday = 0;
            LastResetDate = today;
            UpdatedAtUtc = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Calcula el porcentaje de uso de almacenamiento para un rol específico
    /// </summary>
    public double GetStorageUsagePercentage(UserRole role)
    {
        var quota = Constants.QuotaLimits.GetStorageQuota(role);
        if (quota == 0) return 0;
        return Math.Min(100.0, (double)StorageUsedBytes / quota * 100);
    }

    /// <summary>
    /// Verifica si el usuario está cerca del límite de almacenamiento (>80%)
    /// </summary>
    public bool IsNearStorageLimit(UserRole role)
    {
        return GetStorageUsagePercentage(role) > 80.0;
    }

    /// <summary>
    /// Verifica si el usuario ha excedido su cuota de almacenamiento
    /// </summary>
    public bool HasExceededStorageQuota(UserRole role)
    {
        var quota = Constants.QuotaLimits.GetStorageQuota(role);
        return StorageUsedBytes >= quota;
    }

    /// <summary>
    /// Verifica si el usuario ha excedido su límite diario de subidas
    /// </summary>
    public bool HasExceededDailyUploadLimit(UserRole role)
    {
        ResetDailyCountersIfNeeded();
        var limit = Constants.QuotaLimits.GetFileUploadLimit(role);
        return FilesUploadedToday >= limit;
    }

    /// <summary>
    /// Verifica si el usuario ha excedido su límite de creación de mangas
    /// </summary>
    public bool HasExceededMangaCreationLimit(UserRole role)
    {
        var limit = Constants.QuotaLimits.GetMaxMangaCreations(role);
        return MangasCreated >= limit;
    }

    /// <summary>
    /// Obtiene el espacio de almacenamiento restante en bytes
    /// </summary>
    public long GetRemainingStorageBytes(UserRole role)
    {
        var quota = Constants.QuotaLimits.GetStorageQuota(role);
        return Math.Max(0, quota - StorageUsedBytes);
    }

    /// <summary>
    /// Obtiene el número de subidas restantes para hoy
    /// </summary>
    public int GetRemainingDailyUploads(UserRole role)
    {
        ResetDailyCountersIfNeeded();
        var limit = Constants.QuotaLimits.GetFileUploadLimit(role);
        return Math.Max(0, limit - FilesUploadedToday);
    }

    /// <summary>
    /// Obtiene el número de mangas que aún puede crear
    /// </summary>
    public int GetRemainingMangaCreations(UserRole role)
    {
        var limit = Constants.QuotaLimits.GetMaxMangaCreations(role);
        if (limit == int.MaxValue) return int.MaxValue;
        return Math.Max(0, limit - MangasCreated);
    }
}