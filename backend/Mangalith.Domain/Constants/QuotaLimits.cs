using Mangalith.Domain.Entities;

namespace Mangalith.Domain.Constants;

/// <summary>
/// Define los límites de cuotas y rate limiting por rol de usuario
/// </summary>
public static class QuotaLimits
{
    /// <summary>
    /// Límites de almacenamiento por rol (en bytes)
    /// </summary>
    public static readonly Dictionary<UserRole, long> StorageQuotas = new()
    {
        [UserRole.Reader] = 0, // Los lectores no pueden subir archivos
        [UserRole.Uploader] = 5L * 1024 * 1024 * 1024, // 5GB
        [UserRole.Moderator] = 20L * 1024 * 1024 * 1024, // 20GB
        [UserRole.Administrator] = 100L * 1024 * 1024 * 1024 // 100GB
    };

    /// <summary>
    /// Límites de llamadas API por minuto por rol
    /// </summary>
    public static readonly Dictionary<UserRole, int> ApiCallsPerMinute = new()
    {
        [UserRole.Reader] = 60,
        [UserRole.Uploader] = 120,
        [UserRole.Moderator] = 300,
        [UserRole.Administrator] = 600
    };

    /// <summary>
    /// Límites de subidas de archivos por día por rol
    /// </summary>
    public static readonly Dictionary<UserRole, int> FileUploadsPerDay = new()
    {
        [UserRole.Reader] = 0,
        [UserRole.Uploader] = 10,
        [UserRole.Moderator] = 50,
        [UserRole.Administrator] = 200
    };

    /// <summary>
    /// Tamaño máximo de archivo individual por rol (en bytes)
    /// </summary>
    public static readonly Dictionary<UserRole, long> MaxFileSize = new()
    {
        [UserRole.Reader] = 0,
        [UserRole.Uploader] = 100 * 1024 * 1024, // 100MB
        [UserRole.Moderator] = 500 * 1024 * 1024, // 500MB
        [UserRole.Administrator] = 1024 * 1024 * 1024 // 1GB
    };

    /// <summary>
    /// Número máximo de mangas que puede crear por rol
    /// </summary>
    public static readonly Dictionary<UserRole, int> MaxMangaCreations = new()
    {
        [UserRole.Reader] = 0,
        [UserRole.Uploader] = 20,
        [UserRole.Moderator] = 100,
        [UserRole.Administrator] = int.MaxValue
    };

    /// <summary>
    /// Obtiene la cuota de almacenamiento para un rol específico
    /// </summary>
    public static long GetStorageQuota(UserRole role)
    {
        return StorageQuotas.TryGetValue(role, out var quota) ? quota : 0;
    }

    /// <summary>
    /// Obtiene el límite de llamadas API por minuto para un rol específico
    /// </summary>
    public static int GetApiCallLimit(UserRole role)
    {
        return ApiCallsPerMinute.TryGetValue(role, out var limit) ? limit : 60;
    }

    /// <summary>
    /// Obtiene el límite de subidas de archivos por día para un rol específico
    /// </summary>
    public static int GetFileUploadLimit(UserRole role)
    {
        return FileUploadsPerDay.TryGetValue(role, out var limit) ? limit : 0;
    }

    /// <summary>
    /// Obtiene el tamaño máximo de archivo para un rol específico
    /// </summary>
    public static long GetMaxFileSize(UserRole role)
    {
        return MaxFileSize.TryGetValue(role, out var size) ? size : 0;
    }

    /// <summary>
    /// Obtiene el número máximo de mangas que puede crear un rol específico
    /// </summary>
    public static int GetMaxMangaCreations(UserRole role)
    {
        return MaxMangaCreations.TryGetValue(role, out var limit) ? limit : 0;
    }

    /// <summary>
    /// Verifica si un rol puede subir archivos
    /// </summary>
    public static bool CanUploadFiles(UserRole role)
    {
        return GetStorageQuota(role) > 0;
    }

    /// <summary>
    /// Verifica si un rol puede crear mangas
    /// </summary>
    public static bool CanCreateMangas(UserRole role)
    {
        return GetMaxMangaCreations(role) > 0;
    }
}