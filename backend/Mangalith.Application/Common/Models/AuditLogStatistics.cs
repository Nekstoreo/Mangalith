namespace Mangalith.Application.Common.Models;

/// <summary>
/// Estadísticas de logs de auditoría para dashboards administrativos
/// </summary>
public class AuditLogStatistics
{
    /// <summary>
    /// Número total de logs en el período
    /// </summary>
    public long TotalLogs { get; set; }

    /// <summary>
    /// Número de logs exitosos
    /// </summary>
    public long SuccessfulLogs { get; set; }

    /// <summary>
    /// Número de logs fallidos
    /// </summary>
    public long FailedLogs { get; set; }

    /// <summary>
    /// Porcentaje de éxito
    /// </summary>
    public double SuccessRate => TotalLogs > 0 ? (double)SuccessfulLogs / TotalLogs * 100 : 0;

    /// <summary>
    /// Número de usuarios únicos que realizaron acciones
    /// </summary>
    public long UniqueUsers { get; set; }

    /// <summary>
    /// Número de direcciones IP únicas
    /// </summary>
    public long UniqueIpAddresses { get; set; }

    /// <summary>
    /// Acciones más comunes con su conteo
    /// </summary>
    public Dictionary<string, long> TopActions { get; set; } = new();

    /// <summary>
    /// Recursos más accedidos con su conteo
    /// </summary>
    public Dictionary<string, long> TopResources { get; set; } = new();

    /// <summary>
    /// Usuarios más activos con su conteo de acciones
    /// </summary>
    public Dictionary<string, long> TopUsers { get; set; } = new();

    /// <summary>
    /// Distribución de logs por día
    /// </summary>
    public Dictionary<DateTime, long> LogsByDay { get; set; } = new();

    /// <summary>
    /// Distribución de logs por hora del día
    /// </summary>
    public Dictionary<int, long> LogsByHour { get; set; } = new();

    /// <summary>
    /// Fecha de inicio del período de estadísticas
    /// </summary>
    public DateTime FromDate { get; set; }

    /// <summary>
    /// Fecha de fin del período de estadísticas
    /// </summary>
    public DateTime ToDate { get; set; }

    /// <summary>
    /// Fecha y hora de generación de las estadísticas
    /// </summary>
    public DateTime GeneratedAtUtc { get; set; } = DateTime.UtcNow;
}