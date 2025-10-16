using Mangalith.Application.Common.Models;
using Mangalith.Domain.Entities;

namespace Mangalith.Application.Contracts.Admin;

/// <summary>
/// Response con información detallada de usuario para administradores
/// </summary>
public class UserDetailResponse
{
    /// <summary>
    /// ID único del usuario
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Email del usuario
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Nombre completo del usuario
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Rol actual del usuario
    /// </summary>
    public UserRole Role { get; set; }

    /// <summary>
    /// Estado activo del usuario
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Fecha de creación de la cuenta
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Fecha de última actualización
    /// </summary>
    public DateTime UpdatedAtUtc { get; set; }

    /// <summary>
    /// Fecha de último acceso
    /// </summary>
    public DateTime? LastLoginAtUtc { get; set; }

    /// <summary>
    /// Estadísticas de actividad del usuario
    /// </summary>
    public UserActivityStats ActivityStats { get; set; } = new();

    /// <summary>
    /// Historial de cambios de rol (últimos 10)
    /// </summary>
    public List<UserRoleChangeHistory> RoleHistory { get; set; } = new();
}

/// <summary>
/// Response resumido de usuario para listados
/// </summary>
public class UserSummaryResponse
{
    /// <summary>
    /// ID único del usuario
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Email del usuario
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Nombre completo del usuario
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Rol actual del usuario
    /// </summary>
    public UserRole Role { get; set; }

    /// <summary>
    /// Estado activo del usuario
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Fecha de creación de la cuenta
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Fecha de último acceso
    /// </summary>
    public DateTime? LastLoginAtUtc { get; set; }

    /// <summary>
    /// Número total de archivos subidos
    /// </summary>
    public int TotalUploads { get; set; }
}

/// <summary>
/// Response para operaciones en lote
/// </summary>
public class BulkUserOperationResponse
{
    /// <summary>
    /// Número de usuarios procesados exitosamente
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Número de usuarios que fallaron al procesar
    /// </summary>
    public int FailureCount { get; set; }

    /// <summary>
    /// Lista de errores ocurridos durante el procesamiento
    /// </summary>
    public List<BulkOperationError> Errors { get; set; } = new();

    /// <summary>
    /// Mensaje de resultado general
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Error específico en operación en lote
/// </summary>
public class BulkOperationError
{
    /// <summary>
    /// ID del usuario que causó el error
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Email del usuario para identificación
    /// </summary>
    public string UserEmail { get; set; } = string.Empty;

    /// <summary>
    /// Mensaje de error
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;
}

/// <summary>
/// Estadísticas de actividad de usuario
/// </summary>
public class UserActivityStats
{
    /// <summary>
    /// Total de archivos subidos
    /// </summary>
    public int TotalUploads { get; set; }

    /// <summary>
    /// Total de manga creados
    /// </summary>
    public int TotalMangaCreated { get; set; }

    /// <summary>
    /// Total de capítulos creados
    /// </summary>
    public int TotalChaptersCreated { get; set; }

    /// <summary>
    /// Espacio de almacenamiento utilizado (en bytes)
    /// </summary>
    public long StorageUsed { get; set; }

    /// <summary>
    /// Número de acciones registradas en auditoría (últimos 30 días)
    /// </summary>
    public int RecentAuditActions { get; set; }

    /// <summary>
    /// Fecha de última actividad registrada
    /// </summary>
    public DateTime? LastActivityAtUtc { get; set; }
}

/// <summary>
/// Historial de cambios de rol de usuario
/// </summary>
public class UserRoleChangeHistory
{
    /// <summary>
    /// Rol anterior
    /// </summary>
    public UserRole PreviousRole { get; set; }

    /// <summary>
    /// Nuevo rol
    /// </summary>
    public UserRole NewRole { get; set; }

    /// <summary>
    /// Usuario que realizó el cambio
    /// </summary>
    public string ChangedByUser { get; set; } = string.Empty;

    /// <summary>
    /// Fecha del cambio
    /// </summary>
    public DateTime ChangedAtUtc { get; set; }

    /// <summary>
    /// Razón del cambio
    /// </summary>
    public string? Reason { get; set; }
}

/// <summary>
/// Estadísticas generales del sistema de usuarios
/// </summary>
public class UserSystemStatistics
{
    /// <summary>
    /// Total de usuarios registrados
    /// </summary>
    public int TotalUsers { get; set; }

    /// <summary>
    /// Usuarios activos
    /// </summary>
    public int ActiveUsers { get; set; }

    /// <summary>
    /// Usuarios inactivos
    /// </summary>
    public int InactiveUsers { get; set; }

    /// <summary>
    /// Distribución de usuarios por rol
    /// </summary>
    public Dictionary<UserRole, int> UsersByRole { get; set; } = new();

    /// <summary>
    /// Nuevos usuarios registrados en los últimos 30 días
    /// </summary>
    public int NewUsersLast30Days { get; set; }

    /// <summary>
    /// Usuarios que han iniciado sesión en los últimos 30 días
    /// </summary>
    public int ActiveUsersLast30Days { get; set; }

    /// <summary>
    /// Espacio total de almacenamiento utilizado por todos los usuarios
    /// </summary>
    public long TotalStorageUsed { get; set; }

    /// <summary>
    /// Fecha de generación de las estadísticas
    /// </summary>
    public DateTime GeneratedAtUtc { get; set; } = DateTime.UtcNow;
}