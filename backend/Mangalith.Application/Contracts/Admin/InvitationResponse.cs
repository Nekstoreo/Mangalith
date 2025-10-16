using Mangalith.Application.Common.Models;
using Mangalith.Application.Interfaces.Services;
using Mangalith.Domain.Entities;

namespace Mangalith.Application.Contracts.Admin;

/// <summary>
/// Response detallado de invitación para administradores
/// </summary>
public class InvitationDetailResponse
{
    /// <summary>
    /// ID único de la invitación
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Email del usuario invitado
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Rol objetivo para el usuario invitado
    /// </summary>
    public UserRole TargetRole { get; set; }

    /// <summary>
    /// Token de la invitación (solo para administradores)
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Usuario que creó la invitación
    /// </summary>
    public InvitationUserInfo InvitedBy { get; set; } = new();

    /// <summary>
    /// Fecha de creación de la invitación
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Fecha de expiración de la invitación
    /// </summary>
    public DateTime ExpiresAtUtc { get; set; }

    /// <summary>
    /// Fecha de aceptación (si fue aceptada)
    /// </summary>
    public DateTime? AcceptedAtUtc { get; set; }

    /// <summary>
    /// Usuario que aceptó la invitación (si fue aceptada)
    /// </summary>
    public InvitationUserInfo? AcceptedBy { get; set; }

    /// <summary>
    /// Estado actual de la invitación
    /// </summary>
    public InvitationStatus Status { get; set; }

    /// <summary>
    /// Indica si la invitación está expirada
    /// </summary>
    public bool IsExpired { get; set; }

    /// <summary>
    /// Indica si la invitación fue aceptada
    /// </summary>
    public bool IsAccepted { get; set; }

    /// <summary>
    /// Tiempo restante hasta la expiración
    /// </summary>
    public TimeSpan? TimeUntilExpiration { get; set; }

    /// <summary>
    /// Mensaje personalizado de la invitación
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Número de intentos de envío de email
    /// </summary>
    public int EmailSendAttempts { get; set; }

    /// <summary>
    /// Fecha del último intento de envío de email
    /// </summary>
    public DateTime? LastEmailSentAtUtc { get; set; }
}

/// <summary>
/// Response resumido de invitación para listados
/// </summary>
public class InvitationSummaryResponse
{
    /// <summary>
    /// ID único de la invitación
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Email del usuario invitado
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Rol objetivo para el usuario invitado
    /// </summary>
    public UserRole TargetRole { get; set; }

    /// <summary>
    /// Usuario que creó la invitación
    /// </summary>
    public InvitationUserInfo InvitedBy { get; set; } = new();

    /// <summary>
    /// Fecha de creación de la invitación
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Fecha de expiración de la invitación
    /// </summary>
    public DateTime ExpiresAtUtc { get; set; }

    /// <summary>
    /// Estado actual de la invitación
    /// </summary>
    public InvitationStatus Status { get; set; }

    /// <summary>
    /// Indica si la invitación está expirada
    /// </summary>
    public bool IsExpired { get; set; }

    /// <summary>
    /// Indica si la invitación fue aceptada
    /// </summary>
    public bool IsAccepted { get; set; }

    /// <summary>
    /// Tiempo restante hasta la expiración (en horas)
    /// </summary>
    public double? HoursUntilExpiration { get; set; }
}

/// <summary>
/// Información básica de usuario para invitaciones
/// </summary>
public class InvitationUserInfo
{
    /// <summary>
    /// ID del usuario
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
    /// Rol del usuario
    /// </summary>
    public UserRole Role { get; set; }
}

/// <summary>
/// Response para operaciones en lote sobre invitaciones
/// </summary>
public class BulkInvitationOperationResponse
{
    /// <summary>
    /// Número de invitaciones procesadas exitosamente
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Número de invitaciones que fallaron al procesar
    /// </summary>
    public int FailureCount { get; set; }

    /// <summary>
    /// Lista de errores ocurridos durante el procesamiento
    /// </summary>
    public List<BulkInvitationError> Errors { get; set; } = new();

    /// <summary>
    /// Mensaje de resultado general
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Error específico en operación en lote de invitaciones
/// </summary>
public class BulkInvitationError
{
    /// <summary>
    /// ID de la invitación que causó el error
    /// </summary>
    public Guid InvitationId { get; set; }

    /// <summary>
    /// Email de la invitación para identificación
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Mensaje de error
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;
}

/// <summary>
/// Response para aceptación de invitación
/// </summary>
public class AcceptInvitationResponse
{
    /// <summary>
    /// Indica si la invitación fue aceptada exitosamente
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Mensaje de resultado
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Nuevo rol asignado al usuario
    /// </summary>
    public UserRole? NewRole { get; set; }

    /// <summary>
    /// Información del usuario que aceptó la invitación
    /// </summary>
    public InvitationUserInfo? User { get; set; }

    /// <summary>
    /// Fecha de aceptación
    /// </summary>
    public DateTime? AcceptedAtUtc { get; set; }
}

/// <summary>
/// Response con estadísticas del sistema de invitaciones (usando la clase existente)
/// </summary>
public class InvitationStatisticsResponse : InvitationStatistics
{
    /// <summary>
    /// Invitaciones por estado
    /// </summary>
    public Dictionary<InvitationStatus, long> InvitationsByStatus { get; set; } = new();

    /// <summary>
    /// Invitaciones creadas por mes (últimos 12 meses)
    /// </summary>
    public Dictionary<string, long> InvitationsByMonth { get; set; } = new();

    /// <summary>
    /// Tiempo promedio de aceptación en horas
    /// </summary>
    public double AverageAcceptanceTimeHours { get; set; }

    /// <summary>
    /// Invitaciones que expiran en los próximos 7 días
    /// </summary>
    public long ExpiringInNext7Days { get; set; }
}

/// <summary>
/// Response para validación de invitación por token
/// </summary>
public class ValidateInvitationResponse
{
    /// <summary>
    /// Indica si la invitación es válida
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Información de la invitación (si es válida)
    /// </summary>
    public InvitationDetailResponse? Invitation { get; set; }

    /// <summary>
    /// Mensaje de error (si no es válida)
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Código de error específico
    /// </summary>
    public string? ErrorCode { get; set; }
}