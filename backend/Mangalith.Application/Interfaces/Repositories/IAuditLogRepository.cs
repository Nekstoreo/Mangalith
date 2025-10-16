using Mangalith.Application.Common.Models;
using Mangalith.Domain.Entities;

namespace Mangalith.Application.Interfaces.Repositories;

/// <summary>
/// Repositorio para gestión de logs de auditoría
/// </summary>
public interface IAuditLogRepository
{
    /// <summary>
    /// Crea un nuevo log de auditoría
    /// </summary>
    /// <param name="auditLog">Log de auditoría a crear</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    Task<AuditLog> CreateAsync(AuditLog auditLog, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un log de auditoría por ID
    /// </summary>
    /// <param name="id">ID del log</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Log de auditoría o null si no existe</returns>
    Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene logs de auditoría con filtros y paginación
    /// </summary>
    /// <param name="filter">Filtros para la consulta</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado paginado de logs</returns>
    Task<PagedResult<AuditLog>> GetPagedAsync(AuditLogFilter filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene logs de auditoría para un usuario específico
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <param name="limit">Número máximo de logs</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de logs del usuario</returns>
    Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId, int limit = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cuenta el número total de logs que coinciden con los filtros
    /// </summary>
    /// <param name="filter">Filtros para el conteo</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Número total de logs</returns>
    Task<long> CountAsync(AuditLogFilter filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene estadísticas de auditoría para un período
    /// </summary>
    /// <param name="fromDate">Fecha de inicio</param>
    /// <param name="toDate">Fecha de fin</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Estadísticas de auditoría</returns>
    Task<AuditLogStatistics> GetStatisticsAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina logs de auditoría anteriores a una fecha específica
    /// </summary>
    /// <param name="cutoffDate">Fecha límite (logs anteriores serán eliminados)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Número de logs eliminados</returns>
    Task<long> DeleteOlderThanAsync(DateTime cutoffDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todos los logs que coinciden con los filtros (para exportación)
    /// </summary>
    /// <param name="filter">Filtros para la consulta</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de todos los logs que coinciden</returns>
    Task<IEnumerable<AuditLog>> GetAllAsync(AuditLogFilter filter, CancellationToken cancellationToken = default);
}