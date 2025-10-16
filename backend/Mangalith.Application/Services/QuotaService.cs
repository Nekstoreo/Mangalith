using Microsoft.Extensions.Logging;
using Mangalith.Application.Common.Exceptions;
using Mangalith.Application.Interfaces.Repositories;
using Mangalith.Application.Interfaces.Services;
using Mangalith.Domain.Constants;
using Mangalith.Domain.Entities;

namespace Mangalith.Application.Services;

public class QuotaService : IQuotaService
{
    private readonly IUserQuotaRepository _userQuotaRepository;
    private readonly IRateLimitRepository _rateLimitRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMangaFileRepository _mangaFileRepository;
    private readonly ILogger<QuotaService> _logger;

    public QuotaService(
        IUserQuotaRepository userQuotaRepository,
        IRateLimitRepository rateLimitRepository,
        IUserRepository userRepository,
        IMangaFileRepository mangaFileRepository,
        ILogger<QuotaService> logger)
    {
        _userQuotaRepository = userQuotaRepository;
        _rateLimitRepository = rateLimitRepository;
        _userRepository = userRepository;
        _mangaFileRepository = mangaFileRepository;
        _logger = logger;
    }

    public async Task<bool> CanUploadFileAsync(Guid userId, long fileSize, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("User not found for quota check: {UserId}", userId);
            return false;
        }

        // Verificar si el rol puede subir archivos
        if (!QuotaLimits.CanUploadFiles(user.Role))
        {
            _logger.LogInformation("User {UserId} with role {Role} cannot upload files", userId, user.Role);
            return false;
        }

        // Verificar tamaño máximo de archivo individual
        var maxFileSize = QuotaLimits.GetMaxFileSize(user.Role);
        if (fileSize > maxFileSize)
        {
            _logger.LogInformation("File size {FileSize} exceeds maximum {MaxSize} for user {UserId} with role {Role}", 
                fileSize, maxFileSize, userId, user.Role);
            return false;
        }

        // Verificar cuota de almacenamiento
        var canUpload = await CheckStorageQuotaAsync(userId, fileSize, cancellationToken);
        if (!canUpload)
        {
            _logger.LogInformation("Storage quota exceeded for user {UserId}", userId);
            return false;
        }

        // Verificar límite diario de subidas
        var userQuota = await GetUserQuotaAsync(userId, cancellationToken);
        if (userQuota.HasExceededDailyUploadLimit(user.Role))
        {
            _logger.LogInformation("Daily upload limit exceeded for user {UserId}", userId);
            return false;
        }

        return true;
    }

    public async Task<bool> CheckStorageQuotaAsync(Guid userId, long additionalBytes, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null) return false;

        var userQuota = await GetUserQuotaAsync(userId, cancellationToken);
        var storageQuota = QuotaLimits.GetStorageQuota(user.Role);
        
        return (userQuota.StorageUsedBytes + additionalBytes) <= storageQuota;
    }

    public async Task TrackFileUploadAsync(Guid userId, long fileSize, CancellationToken cancellationToken = default)
    {
        var userQuota = await GetUserQuotaAsync(userId, cancellationToken);
        
        userQuota.AddStorageUsage(fileSize);
        userQuota.IncrementFileUpload();
        
        await _userQuotaRepository.UpdateAsync(userQuota, cancellationToken);
        
        _logger.LogInformation("Tracked file upload for user {UserId}: {FileSize} bytes", userId, fileSize);
    }

    public async Task TrackFileDeleteAsync(Guid userId, long fileSize, CancellationToken cancellationToken = default)
    {
        var userQuota = await GetUserQuotaAsync(userId, cancellationToken);
        
        userQuota.RemoveStorageUsage(fileSize);
        
        await _userQuotaRepository.UpdateAsync(userQuota, cancellationToken);
        
        _logger.LogInformation("Tracked file deletion for user {UserId}: {FileSize} bytes", userId, fileSize);
    }

    public async Task<UserQuota> GetUserQuotaAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var userQuota = await _userQuotaRepository.GetByUserIdAsync(userId, cancellationToken);
        
        if (userQuota == null)
        {
            userQuota = new UserQuota(userId);
            await _userQuotaRepository.CreateAsync(userQuota, cancellationToken);
            _logger.LogInformation("Created new quota record for user {UserId}", userId);
        }
        else
        {
            // Resetear contadores diarios si es necesario
            userQuota.ResetDailyCountersIfNeeded();
            if (userQuota.LastResetDate < DateTime.UtcNow.Date)
            {
                await _userQuotaRepository.UpdateAsync(userQuota, cancellationToken);
            }
        }
        
        return userQuota;
    }

    public async Task<bool> CanCreateMangaAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null) return false;

        if (!QuotaLimits.CanCreateMangas(user.Role))
        {
            return false;
        }

        var userQuota = await GetUserQuotaAsync(userId, cancellationToken);
        return !userQuota.HasExceededMangaCreationLimit(user.Role);
    }

    public async Task TrackMangaCreationAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var userQuota = await GetUserQuotaAsync(userId, cancellationToken);
        
        userQuota.IncrementMangaCreation();
        
        await _userQuotaRepository.UpdateAsync(userQuota, cancellationToken);
        
        _logger.LogInformation("Tracked manga creation for user {UserId}", userId);
    }

    public async Task TrackMangaDeletionAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var userQuota = await GetUserQuotaAsync(userId, cancellationToken);
        
        userQuota.DecrementMangaCreation();
        
        await _userQuotaRepository.UpdateAsync(userQuota, cancellationToken);
        
        _logger.LogInformation("Tracked manga deletion for user {UserId}", userId);
    }

    public async Task<bool> CheckRateLimitAsync(Guid userId, string endpoint, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null) return false;

        var rateLimitEntry = await _rateLimitRepository.GetByUserAndEndpointAsync(userId, endpoint, cancellationToken);
        
        if (rateLimitEntry == null)
        {
            return true; // No hay entrada previa, permitir
        }

        if (rateLimitEntry.IsWindowExpired())
        {
            return true; // Ventana expirada, permitir
        }

        return !rateLimitEntry.HasExceededLimit(user.Role);
    }

    public async Task TrackApiCallAsync(Guid userId, string endpoint, CancellationToken cancellationToken = default)
    {
        var rateLimitEntry = await _rateLimitRepository.GetByUserAndEndpointAsync(userId, endpoint, cancellationToken);
        
        if (rateLimitEntry == null)
        {
            rateLimitEntry = new RateLimitEntry(userId, endpoint);
            await _rateLimitRepository.CreateAsync(rateLimitEntry, cancellationToken);
        }
        else if (rateLimitEntry.IsWindowExpired())
        {
            rateLimitEntry.ResetWindow();
            await _rateLimitRepository.UpdateAsync(rateLimitEntry, cancellationToken);
        }
        else
        {
            rateLimitEntry.IncrementRequest();
            await _rateLimitRepository.UpdateAsync(rateLimitEntry, cancellationToken);
        }
    }

    public async Task<QuotaUsageReport> GetQuotaUsageReportAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException($"User with ID {userId} not found");
        }

        var userQuota = await GetUserQuotaAsync(userId, cancellationToken);
        var storageQuota = QuotaLimits.GetStorageQuota(user.Role);
        var dailyUploadLimit = QuotaLimits.GetFileUploadLimit(user.Role);
        var mangaCreationLimit = QuotaLimits.GetMaxMangaCreations(user.Role);

        return new QuotaUsageReport
        {
            UserId = userId,
            UserRole = user.Role,
            StorageUsedBytes = userQuota.StorageUsedBytes,
            StorageQuotaBytes = storageQuota,
            StorageUsagePercentage = userQuota.GetStorageUsagePercentage(user.Role),
            FilesUploadedToday = userQuota.FilesUploadedToday,
            DailyUploadLimit = dailyUploadLimit,
            MangasCreated = userQuota.MangasCreated,
            MangaCreationLimit = mangaCreationLimit,
            IsNearStorageLimit = userQuota.IsNearStorageLimit(user.Role),
            HasExceededAnyLimit = userQuota.HasExceededStorageQuota(user.Role) || 
                                 userQuota.HasExceededDailyUploadLimit(user.Role) || 
                                 userQuota.HasExceededMangaCreationLimit(user.Role),
            LastUpdated = userQuota.UpdatedAtUtc
        };
    }

    public async Task<IEnumerable<UserQuota>> GetUsersNearLimitAsync(double thresholdPercentage = 80.0, CancellationToken cancellationToken = default)
    {
        return await _userQuotaRepository.GetUsersNearLimitAsync(thresholdPercentage, cancellationToken);
    }

    public async Task<QuotaSystemStats> GetSystemQuotaStatsAsync(CancellationToken cancellationToken = default)
    {
        var totalStorageUsed = await _userQuotaRepository.GetTotalStorageUsageAsync(cancellationToken);
        var storageByRole = await _userQuotaRepository.GetStorageUsageByRoleAsync(cancellationToken);
        var usersNearLimit = await _userQuotaRepository.GetUsersNearLimitAsync(80.0, cancellationToken);
        var usersExceedingLimit = await _userQuotaRepository.GetUsersExceedingLimitAsync(cancellationToken);

        // Obtener conteo de usuarios por rol (esto requeriría una consulta adicional al repositorio de usuarios)
        var userCountByRole = new Dictionary<UserRole, int>();
        foreach (UserRole role in Enum.GetValues<UserRole>())
        {
            userCountByRole[role] = 0; // Placeholder - implementar consulta real si es necesario
        }

        return new QuotaSystemStats
        {
            TotalStorageUsed = totalStorageUsed,
            TotalUsers = userCountByRole.Values.Sum(),
            UsersNearLimit = usersNearLimit.Count(),
            UsersExceedingLimit = usersExceedingLimit.Count(),
            StorageByRole = storageByRole,
            UserCountByRole = userCountByRole,
            GeneratedAt = DateTime.UtcNow
        };
    }

    public async Task CleanupExpiredRateLimitEntriesAsync(CancellationToken cancellationToken = default)
    {
        await _rateLimitRepository.DeleteExpiredEntriesAsync(cancellationToken);
        _logger.LogInformation("Cleaned up expired rate limit entries");
    }

    public async Task RecalculateUserStorageUsageAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var userFiles = await _mangaFileRepository.GetByUserIdAsync(userId, cancellationToken);
        var totalSize = userFiles.Sum(f => f.FileSize);

        var userQuota = await GetUserQuotaAsync(userId, cancellationToken);
        
        // Actualizar el uso de almacenamiento con el valor recalculado
        var oldUsage = userQuota.StorageUsedBytes;
        userQuota.RemoveStorageUsage(oldUsage); // Resetear a 0
        userQuota.AddStorageUsage(totalSize); // Establecer el valor correcto

        await _userQuotaRepository.UpdateAsync(userQuota, cancellationToken);

        _logger.LogInformation("Recalculated storage usage for user {UserId}: {OldUsage} -> {NewUsage} bytes", 
            userId, oldUsage, totalSize);
    }
}