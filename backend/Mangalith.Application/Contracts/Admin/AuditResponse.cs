using Mangalith.Application.Common.Models;
using Mangalith.Domain.Entities;

namespace Mangalith.Application.Contracts.Admin;

/// <summary>
/// Response para logs de auditoría con información adicional
/// </summary>
public class AuditLogResponse
{
    /// <summary>
    /// ID único del log de auditoría
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID del usuario que realizó la acción
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Información del usuario que realizó la acción
    /// </summary>
    public AuditUserInfo User { get; set; } = new();

    /// <summary>
    /// Acción realizada
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Recurso afectado
    /// </summary>
    public string Resource { get; set; } = string.Empty;

    /// <summary>
    /// ID específico del recurso (opcional)
    /// </summary>
    public string? ResourceId { get; set; }

    /// <summary>
    /// Detalles adicionales de la acción
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Dirección IP desde donde se realizó la acción
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// User agent del navegador
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Indica si la acción fue exitosa
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Fecha y hora de la acción
    /// </summary>
    public DateTime TimestampUtc { get; set; }

    /// <summary>
    /// Severidad del evento (calculada)
    /// </summary>
    public AuditSeverity Severity { get; set; }
}

/// <summary>
/// Información básica del usuario para logs de auditoría
/// </summary>
public class AuditUserInfo
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
    /// Rol del usuario al momento de la acción
    /// </summary>
    public UserRole Role { get; set; }
}

/// <summary>
/// Severidad de eventos de auditoría
/// </summary>
public enum AuditSeverity
{
    /// <summary>
    /// Información general
    /// </summary>
    Info,

    /// <summary>
    /// Advertencia
    /// </summary>
    Warning,

    /// <summary>
    /// Error
    /// </summary>
    Error,

    /// <summary>
    /// Crítico
    /// </summary>
    Critical
}

/// <summary>
/// Response para exportación de logs de auditoría
/// </summary>
public class AuditExportResponse
{
    /// <summary>
    /// Nombre del archivo generado
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de contenido del archivo
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Tamaño del archivo en bytes
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Número de registros exportados
    /// </summary>
    public int RecordCount { get; set; }

    /// <summary>
    /// Fecha de generación del archivo
    /// </summary>
    public DateTime GeneratedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Filtros aplicados en la exportación
    /// </summary>
    public string? AppliedFilters { get; set; }
}

/// <summary>
/// Request para exportación de logs de auditoría
/// </summary>
public class AuditExportRequest
{
    /// <summary>
    /// Formato de exportación (CSV, JSON, Excel)
    /// </summary>
    public string Format { get; set; } = "CSV";

    /// <summary>
    /// Incluir detalles completos en la exportación
    /// </summary>
    public bool IncludeDetails { get; set; } = true;

    /// <summary>
    /// Incluir información de usuario en la exportación
    /// </summary>
    public bool IncludeUserInfo { get; set; } = true;

    /// <summary>
    /// Filtros a aplicar (opcional)
    /// </summary>
    public AuditLogFilter? Filter { get; set; }
}

/// <summary>
/// Response con resumen de actividad de auditoría
/// </summary>
public class AuditActivitySummary
{
    /// <summary>
    /// Actividad en las últimas 24 horas
    /// </summary>
    public AuditPeriodSummary Last24Hours { get; set; } = new();

    /// <summary>
    /// Actividad en los últimos 7 días
    /// </summary>
    public AuditPeriodSummary Last7Days { get; set; } = new();

    /// <summary>
    /// Actividad en los últimos 30 días
    /// </summary>
    public AuditPeriodSummary Last30Days { get; set; } = new();

    /// <summary>
    /// Alertas de seguridad recientes
    /// </summary>
    public List<SecurityAlert> SecurityAlerts { get; set; } = new();

    /// <summary>
    /// Fecha de generación del resumen
    /// </summary>
    public DateTime GeneratedAtUtc { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Resumen de actividad para un período específico
/// </summary>
public class AuditPeriodSummary
{
    /// <summary>
    /// Total de eventos
    /// </summary>
    public long TotalEvents { get; set; }

    /// <summary>
    /// Eventos exitosos
    /// </summary>
    public long SuccessfulEvents { get; set; }

    /// <summary>
    /// Eventos fallidos
    /// </summary>
    public long FailedEvents { get; set; }

    /// <summary>
    /// Usuarios únicos activos
    /// </summary>
    public long UniqueUsers { get; set; }

    /// <summary>
    /// Acciones más comunes
    /// </summary>
    public Dictionary<string, long> TopActions { get; set; } = new();
}

/// <summary>
/// Alerta de seguridad basada en logs de auditoría
/// </summary>
public class SecurityAlert
{
    /// <summary>
    /// ID único de la alerta
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Tipo de alerta
    /// </summary>
    public SecurityAlertType Type { get; set; }

    /// <summary>
    /// Severidad de la alerta
    /// </summary>
    public AuditSeverity Severity { get; set; }

    /// <summary>
    /// Mensaje descriptivo de la alerta
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Usuario relacionado con la alerta (opcional)
    /// </summary>
    public AuditUserInfo? User { get; set; }

    /// <summary>
    /// Dirección IP relacionada (opcional)
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// Número de eventos relacionados
    /// </summary>
    public int EventCount { get; set; }

    /// <summary>
    /// Fecha del primer evento relacionado
    /// </summary>
    public DateTime FirstEventUtc { get; set; }

    /// <summary>
    /// Fecha del último evento relacionado
    /// </summary>
    public DateTime LastEventUtc { get; set; }
}

/// <summary>
/// Tipos de alertas de seguridad
/// </summary>
public enum SecurityAlertType
{
    /// <summary>
    /// Múltiples intentos de acceso fallidos
    /// </summary>
    MultipleFailedAttempts,

    /// <summary>
    /// Acceso desde IP sospechosa
    /// </summary>
    SuspiciousIpAccess,

    /// <summary>
    /// Cambios de rol inusuales
    /// </summary>
    UnusualRoleChanges,

    /// <summary>
    /// Actividad fuera de horario normal
    /// </summary>
    OffHoursActivity,

    /// <summary>
    /// Eliminación masiva de datos
    /// </summary>
    MassDataDeletion,

    /// <summary>
    /// Acceso a recursos sensibles
    /// </summary>
    SensitiveResourceAccess
}