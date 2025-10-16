using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Mangalith.Application.Common.Models;
using Mangalith.Application.Interfaces.Repositories;
using Mangalith.Application.Interfaces.Services;
using Mangalith.Domain.Entities;

namespace Mangalith.Application.Services;

/// <summary>
/// Servicio para gestión de logs de auditoría con capacidades de filtrado y estadísticas
/// </summary>
public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ILogger<AuditService> _logger;

    public AuditService(
        IAuditLogRepository auditLogRepository,
        ILogger<AuditService> logger)
    {
        _auditLogRepository = auditLogRepository;
        _logger = logger;
    }

    public async Task LogActionAsync(
        Guid userId,
        string action,
        string resource,
        string ipAddress,
        bool success = true,
        string? resourceId = null,
        string? details = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var auditLog = new AuditLog(
                userId,
                action,
                resource,
                ipAddress,
                success,
                resourceId,
                details,
                userAgent);

            await _auditLogRepository.CreateAsync(auditLog, cancellationToken);

            _logger.LogInformation(
                "Audit log created: User {UserId} performed {Action} on {Resource} ({ResourceId}) - Success: {Success}",
                userId, action, resource, resourceId ?? "N/A", success);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to create audit log for User {UserId}, Action {Action}, Resource {Resource}",
                userId, action, resource);
            
            // No relanzamos la excepción para evitar que falle la operación principal
            // El logging de auditoría no debe interrumpir el flujo normal
        }
    }

    public async Task LogSuccessAsync(
        Guid userId,
        string action,
        string resource,
        string ipAddress,
        string? resourceId = null,
        string? details = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default)
    {
        await LogActionAsync(userId, action, resource, ipAddress, true, resourceId, details, userAgent, cancellationToken);
    }

    public async Task LogFailureAsync(
        Guid userId,
        string action,
        string resource,
        string ipAddress,
        string? resourceId = null,
        string? details = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default)
    {
        await LogActionAsync(userId, action, resource, ipAddress, false, resourceId, details, userAgent, cancellationToken);
    }

    public async Task<PagedResult<AuditLog>> GetAuditLogsAsync(AuditLogFilter filter, CancellationToken cancellationToken = default)
    {
        try
        {
            filter.Validate();
            var result = await _auditLogRepository.GetPagedAsync(filter, cancellationToken);
            
            _logger.LogDebug("Retrieved {Count} audit logs (page {Page} of {TotalPages})",
                result.Items.Count(), result.Page, result.TotalPages);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs with filter");
            return PagedResult<AuditLog>.Empty(filter.Page, filter.PageSize);
        }
    }

    public async Task<AuditLog?> GetAuditLogByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var auditLog = await _auditLogRepository.GetByIdAsync(id, cancellationToken);
            
            if (auditLog == null)
            {
                _logger.LogWarning("Audit log with ID {AuditLogId} not found", id);
            }
            
            return auditLog;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit log with ID {AuditLogId}", id);
            return null;
        }
    }

    public async Task<AuditLogStatistics> GetAuditStatisticsAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validar rango de fechas
            if (fromDate > toDate)
            {
                var temp = fromDate;
                fromDate = toDate;
                toDate = temp;
            }

            var statistics = await _auditLogRepository.GetStatisticsAsync(fromDate, toDate, cancellationToken);
            
            _logger.LogDebug("Generated audit statistics for period {FromDate} to {ToDate}: {TotalLogs} total logs",
                fromDate, toDate, statistics.TotalLogs);
            
            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating audit statistics for period {FromDate} to {ToDate}", fromDate, toDate);
            
            return new AuditLogStatistics
            {
                FromDate = fromDate,
                ToDate = toDate,
                GeneratedAtUtc = DateTime.UtcNow
            };
        }
    }

    public async Task<AuditLogStatistics> GetRecentAuditStatisticsAsync(int days = 30, CancellationToken cancellationToken = default)
    {
        var toDate = DateTime.UtcNow;
        var fromDate = toDate.AddDays(-Math.Abs(days));
        
        return await GetAuditStatisticsAsync(fromDate, toDate, cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> GetRecentUserActivityAsync(Guid userId, int limit = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            // Limitar el número máximo de logs
            limit = Math.Min(Math.Max(limit, 1), 100);
            
            var userLogs = await _auditLogRepository.GetByUserIdAsync(userId, limit, cancellationToken);
            
            _logger.LogDebug("Retrieved {Count} recent activity logs for user {UserId}", userLogs.Count(), userId);
            
            return userLogs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recent activity for user {UserId}", userId);
            return Enumerable.Empty<AuditLog>();
        }
    }

    public async Task<long> CleanupOldLogsAsync(int retentionDays = 365, CancellationToken cancellationToken = default)
    {
        try
        {
            // Asegurar que el período de retención sea razonable
            retentionDays = Math.Max(retentionDays, 30); // Mínimo 30 días
            
            var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);
            var deletedCount = await _auditLogRepository.DeleteOlderThanAsync(cutoffDate, cancellationToken);
            
            _logger.LogInformation("Cleaned up {DeletedCount} audit logs older than {CutoffDate}", deletedCount, cutoffDate);
            
            return deletedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up old audit logs with retention period {RetentionDays} days", retentionDays);
            return 0;
        }
    }

    public async Task<Stream> ExportAuditLogsAsync(AuditLogFilter filter, string format = "CSV", CancellationToken cancellationToken = default)
    {
        try
        {
            filter.Validate();
            var auditLogs = await _auditLogRepository.GetAllAsync(filter, cancellationToken);
            
            var stream = new MemoryStream();
            
            switch (format.ToUpperInvariant())
            {
                case "JSON":
                    await ExportAsJsonAsync(auditLogs, stream, cancellationToken);
                    break;
                case "CSV":
                default:
                    await ExportAsCsvAsync(auditLogs, stream, cancellationToken);
                    break;
            }
            
            stream.Position = 0;
            
            _logger.LogInformation("Exported {Count} audit logs in {Format} format", auditLogs.Count(), format);
            
            return stream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting audit logs in {Format} format", format);
            
            // Retornar un stream vacío en caso de error
            var emptyStream = new MemoryStream();
            return emptyStream;
        }
    }

    private async Task ExportAsCsvAsync(IEnumerable<AuditLog> auditLogs, Stream stream, CancellationToken cancellationToken)
    {
        using var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true);
        
        // Escribir encabezados
        await writer.WriteLineAsync("Id,UserId,UserEmail,Action,Resource,ResourceId,Success,IpAddress,UserAgent,Details,TimestampUtc");
        
        // Escribir datos
        foreach (var log in auditLogs)
        {
            var line = $"{log.Id}," +
                      $"{log.UserId}," +
                      $"\"{log.User?.Email ?? "N/A"}\"," +
                      $"\"{log.Action}\"," +
                      $"\"{log.Resource}\"," +
                      $"\"{log.ResourceId ?? ""}\"," +
                      $"{log.Success}," +
                      $"\"{log.IpAddress}\"," +
                      $"\"{log.UserAgent ?? ""}\"," +
                      $"\"{log.Details?.Replace("\"", "\"\"") ?? ""}\"," +
                      $"{log.TimestampUtc:yyyy-MM-dd HH:mm:ss}";
            
            await writer.WriteLineAsync(line);
            
            if (cancellationToken.IsCancellationRequested)
                break;
        }
        
        await writer.FlushAsync();
    }

    private async Task ExportAsJsonAsync(IEnumerable<AuditLog> auditLogs, Stream stream, CancellationToken cancellationToken)
    {
        var exportData = auditLogs.Select(log => new
        {
            log.Id,
            log.UserId,
            UserEmail = log.User?.Email,
            log.Action,
            log.Resource,
            log.ResourceId,
            log.Success,
            log.IpAddress,
            log.UserAgent,
            log.Details,
            log.TimestampUtc
        });

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await JsonSerializer.SerializeAsync(stream, exportData, options, cancellationToken);
    }
}