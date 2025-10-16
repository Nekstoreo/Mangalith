using Mangalith.Application.Common.Models;
using Mangalith.Domain.Entities;

namespace Mangalith.Application.Interfaces.Services;

/// <summary>
/// Servicio para gestión de logs de auditoría del sistema
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Registra una acción en el log de auditoría
    /// </summary>
    /// <param name="userId">ID del usuario que realizó la acción</param>
    /// <param name="action">Acción realizada</param>
    /// <param name="resource">Recurso afectado</param>
    /// <param name="ipAddress">Dirección IP del usuario</param>
    /// <param name="success">Indica si la acción fue exitosa</param>
    /// <param name="resourceId">ID específico del recurso (opcional)</param>
    /// <param name="details">Detalles adicionales de la acción (opcional)</param>
    /// <param name="userAgent">User agent del navegador (opcional)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    Task LogActionAsync(
        Guid userId,
        string action,
        string resource,
        string ipAddress,
        bool success = true,
        string? resourceId = null,
        string? details = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra una acción exitosa en el log de auditoría
    /// </summary>
    /// <param name="userId">ID del usuario que realizó la acción</param>
    /// <param name="action">Acción realizada</param>
    /// <param name="resource">Recurso afectado</param>
    /// <param name="ipAddress">Dirección IP del usuario</param>
    /// <param name="resourceId">ID específico del recurso (opcional)</param>
    /// <param name="details">Detalles adicionales de la acción (opcional)</param>
    /// <param name="userAgent">User agent del navegador (opcional)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    Task LogSuccessAsync(
        Guid userId,
        string action,
        string resource,
        string ipAddress,
        string? resourceId = null,
        string? details = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra una acción fallida en el log de auditoría
    /// </summary>
    /// <param name="userId">ID del usuario que realizó la acción</param>
    /// <param name="action">Acción realizada</param>
    /// <param name="resource">Recurso afectado</param>
    /// <param name="ipAddress">Dirección IP del usuario</param>
    /// <param name="resourceId">ID específico del recurso (opcional)</param>
    /// <param name="details">Detalles adicionales del fallo (opcional)</param>
    /// <param name="userAgent">User agent del navegador (opcional)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    Task LogFailureAsync(
        Guid userId,
        string action,
        string resource,
        string ipAddress,
        string? resourceId = null,
        string? details = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene logs de auditoría con filtros y paginación
    /// </summary>
    /// <param name="filter">Filtros para la consulta</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado paginado de logs de auditoría</returns>
    Task<PagedResult<AuditLog>> GetAuditLogsAsync(AuditLogFilter filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un log de auditoría específico por ID
    /// </summary>
    /// <param name="id">ID del log de auditoría</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Log de auditoría o null si no existe</returns>
    Task<AuditLog?> GetAuditLogByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Genera estadísticas de auditoría para un período específico
    /// </summary>
    /// <param name="fromDate">Fecha de inicio del período</param>
    /// <param name="toDate">Fecha de fin del período</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Estadísticas de auditoría</returns>
    Task<AuditLogStatistics> GetAuditStatisticsAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene estadísticas de auditoría para los últimos N días
    /// </summary>
    /// <param name="days">Número de días hacia atrás</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Estadísticas de auditoría</returns>
    Task<AuditLogStatistics> GetRecentAuditStatisticsAsync(int days = 30, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene los logs de auditoría más recientes para un usuario específico
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <param name="limit">Número máximo de logs a retornar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de logs de auditoría recientes</returns>
    Task<IEnumerable<AuditLog>> GetRecentUserActivityAsync(Guid userId, int limit = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Limpia logs de auditoría antiguos según la política de retención
    /// </summary>
    /// <param name="retentionDays">Días de retención (logs más antiguos serán eliminados)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Número de logs eliminados</returns>
    Task<long> CleanupOldLogsAsync(int retentionDays = 365, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exporta logs de auditoría a un formato específico
    /// </summary>
    /// <param name="filter">Filtros para la exportación</param>
    /// <param name="format">Formato de exportación (CSV, JSON)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Datos exportados como stream</returns>
    Task<Stream> ExportAuditLogsAsync(AuditLogFilter filter, string format = "CSV", CancellationToken cancellationToken = default);
}