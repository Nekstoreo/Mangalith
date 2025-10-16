using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mangalith.Api.Authorization;
using Mangalith.Api.Contracts;
using Mangalith.Application.Common.Models;
using Mangalith.Application.Contracts.Admin;
using Mangalith.Application.Interfaces.Services;
using Mangalith.Domain.Constants;
using Mangalith.Domain.Entities;

namespace Mangalith.Api.Controllers;

/// <summary>
/// Controlador para gestión de logs de auditoría
/// </summary>
[ApiController]
[Route("api/admin/audit")]
[Authorize]
public class AuditController : ControllerBase
{
    private readonly IAuditService _auditService;
    private readonly ILogger<AuditController> _logger;

    public AuditController(
        IAuditService auditService,
        ILogger<AuditController> logger)
    {
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene logs de auditoría con filtros y paginación
    /// </summary>
    [HttpGet("logs")]
    [RequirePermission(Permissions.System.Audit)]
    [ProducesResponseType(typeof(PagedResult<AuditLogResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAuditLogs([FromQuery] AuditLogFilter filter, CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} requesting audit logs with filter: {@Filter}", currentUserId, filter);

            // Validar y ajustar filtros
            filter.Validate();

            var result = await _auditService.GetAuditLogsAsync(filter, cancellationToken);

            // Convertir a response con información adicional
            var auditLogResponses = result.Items.Select(log => new AuditLogResponse
            {
                Id = log.Id,
                UserId = log.UserId,
                User = new AuditUserInfo
                {
                    Id = log.User.Id,
                    Email = log.User.Email,
                    FullName = log.User.FullName,
                    Role = log.User.Role
                },
                Action = log.Action,
                Resource = log.Resource,
                ResourceId = log.ResourceId,
                Details = log.Details,
                IpAddress = log.IpAddress,
                UserAgent = log.UserAgent,
                Success = log.Success,
                TimestampUtc = log.TimestampUtc,
                Severity = DetermineSeverity(log)
            }).ToList();

            var response = new PagedResult<AuditLogResponse>(auditLogResponses, result.TotalCount, result.Page, result.PageSize);

            // Registrar acceso a logs de auditoría
            await _auditService.LogSuccessAsync(
                currentUserId,
                "audit.view_logs",
                "audit_system",
                GetClientIpAddress(),
                resourceId: null,
                details: $"Retrieved {auditLogResponses.Count} audit logs (page {filter.Page})",
                cancellationToken: cancellationToken);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs");
            return StatusCode(500, new ErrorResponse("internal_error", "Error retrieving audit logs"));
        }
    }

    /// <summary>
    /// Obtiene un log de auditoría específico por ID
    /// </summary>
    [HttpGet("logs/{logId:guid}")]
    [RequirePermission(Permissions.System.Audit)]
    [ProducesResponseType(typeof(AuditLogResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAuditLog(Guid logId, CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} requesting audit log {LogId}", currentUserId, logId);

            var auditLog = await _auditService.GetAuditLogByIdAsync(logId, cancellationToken);
            if (auditLog == null)
            {
                return NotFound(new ErrorResponse("audit_log_not_found", "Audit log not found"));
            }

            var response = new AuditLogResponse
            {
                Id = auditLog.Id,
                UserId = auditLog.UserId,
                User = new AuditUserInfo
                {
                    Id = auditLog.User.Id,
                    Email = auditLog.User.Email,
                    FullName = auditLog.User.FullName,
                    Role = auditLog.User.Role
                },
                Action = auditLog.Action,
                Resource = auditLog.Resource,
                ResourceId = auditLog.ResourceId,
                Details = auditLog.Details,
                IpAddress = auditLog.IpAddress,
                UserAgent = auditLog.UserAgent,
                Success = auditLog.Success,
                TimestampUtc = auditLog.TimestampUtc,
                Severity = DetermineSeverity(auditLog)
            };

            // Registrar acceso a log específico
            await _auditService.LogSuccessAsync(
                currentUserId,
                "audit.view_log_detail",
                "audit_log",
                GetClientIpAddress(),
                logId.ToString(),
                $"Viewed audit log detail for {auditLog.Action}",
                cancellationToken: cancellationToken);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit log {LogId}", logId);
            return StatusCode(500, new ErrorResponse("internal_error", "Error retrieving audit log"));
        }
    }

    /// <summary>
    /// Obtiene estadísticas de auditoría para un período específico
    /// </summary>
    [HttpGet("statistics")]
    [RequirePermission(Permissions.System.Audit)]
    [ProducesResponseType(typeof(AuditLogStatistics), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAuditStatistics(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} requesting audit statistics from {FromDate} to {ToDate}", 
                currentUserId, fromDate, toDate);

            // Usar valores por defecto si no se proporcionan fechas
            var from = fromDate ?? DateTime.UtcNow.AddDays(-30);
            var to = toDate ?? DateTime.UtcNow;

            var statistics = await _auditService.GetAuditStatisticsAsync(from, to, cancellationToken);

            // Registrar acceso a estadísticas
            await _auditService.LogSuccessAsync(
                currentUserId,
                "audit.view_statistics",
                "audit_system",
                GetClientIpAddress(),
                details: $"Viewed audit statistics for period {from:yyyy-MM-dd} to {to:yyyy-MM-dd}",
                cancellationToken: cancellationToken);

            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit statistics");
            return StatusCode(500, new ErrorResponse("internal_error", "Error retrieving audit statistics"));
        }
    }

    /// <summary>
    /// Obtiene estadísticas recientes de auditoría (últimos 30 días)
    /// </summary>
    [HttpGet("statistics/recent")]
    [RequirePermission(Permissions.System.Audit)]
    [ProducesResponseType(typeof(AuditLogStatistics), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetRecentAuditStatistics(CancellationToken cancellationToken, [FromQuery] int days = 30)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} requesting recent audit statistics for {Days} days", currentUserId, days);

            // Limitar el rango de días
            days = Math.Max(1, Math.Min(days, 365));

            var statistics = await _auditService.GetRecentAuditStatisticsAsync(days, cancellationToken);

            // Registrar acceso a estadísticas recientes
            await _auditService.LogSuccessAsync(
                currentUserId,
                "audit.view_recent_statistics",
                "audit_system",
                GetClientIpAddress(),
                details: $"Viewed recent audit statistics for {days} days",
                cancellationToken: cancellationToken);

            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recent audit statistics");
            return StatusCode(500, new ErrorResponse("internal_error", "Error retrieving recent audit statistics"));
        }
    }

    /// <summary>
    /// Obtiene resumen de actividad de auditoría
    /// </summary>
    [HttpGet("activity-summary")]
    [RequirePermission(Permissions.System.Audit)]
    [ProducesResponseType(typeof(AuditActivitySummary), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetActivitySummary(CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} requesting audit activity summary", currentUserId);

            // Obtener estadísticas para diferentes períodos
            var last24Hours = await _auditService.GetRecentAuditStatisticsAsync(1, cancellationToken);
            var last7Days = await _auditService.GetRecentAuditStatisticsAsync(7, cancellationToken);
            var last30Days = await _auditService.GetRecentAuditStatisticsAsync(30, cancellationToken);

            var summary = new AuditActivitySummary
            {
                Last24Hours = new AuditPeriodSummary
                {
                    TotalEvents = last24Hours.TotalLogs,
                    SuccessfulEvents = last24Hours.SuccessfulLogs,
                    FailedEvents = last24Hours.FailedLogs,
                    UniqueUsers = last24Hours.UniqueUsers,
                    TopActions = last24Hours.TopActions
                },
                Last7Days = new AuditPeriodSummary
                {
                    TotalEvents = last7Days.TotalLogs,
                    SuccessfulEvents = last7Days.SuccessfulLogs,
                    FailedEvents = last7Days.FailedLogs,
                    UniqueUsers = last7Days.UniqueUsers,
                    TopActions = last7Days.TopActions
                },
                Last30Days = new AuditPeriodSummary
                {
                    TotalEvents = last30Days.TotalLogs,
                    SuccessfulEvents = last30Days.SuccessfulLogs,
                    FailedEvents = last30Days.FailedLogs,
                    UniqueUsers = last30Days.UniqueUsers,
                    TopActions = last30Days.TopActions
                },
                SecurityAlerts = await GenerateSecurityAlerts(cancellationToken),
                GeneratedAtUtc = DateTime.UtcNow
            };

            // Registrar acceso al resumen de actividad
            await _auditService.LogSuccessAsync(
                currentUserId,
                "audit.view_activity_summary",
                "audit_system",
                GetClientIpAddress(),
                details: "Viewed audit activity summary",
                cancellationToken: cancellationToken);

            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit activity summary");
            return StatusCode(500, new ErrorResponse("internal_error", "Error retrieving audit activity summary"));
        }
    }

    /// <summary>
    /// Exporta logs de auditoría en el formato especificado
    /// </summary>
    [HttpPost("export")]
    [RequirePermission(Permissions.System.Audit)]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportAuditLogs([FromBody] AuditExportRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} requesting audit logs export in format {Format}", currentUserId, request.Format);

            // Validar formato
            var supportedFormats = new[] { "CSV", "JSON", "Excel" };
            if (!supportedFormats.Contains(request.Format.ToUpper()))
            {
                return BadRequest(new ErrorResponse("invalid_format", $"Supported formats: {string.Join(", ", supportedFormats)}"));
            }

            // Usar filtro por defecto si no se proporciona
            var filter = request.Filter ?? new AuditLogFilter();
            filter.Validate();

            // Exportar logs
            var exportStream = await _auditService.ExportAuditLogsAsync(filter, request.Format.ToUpper(), cancellationToken);

            // Determinar tipo de contenido y nombre de archivo
            var (contentType, fileExtension) = request.Format.ToUpper() switch
            {
                "CSV" => ("text/csv", "csv"),
                "JSON" => ("application/json", "json"),
                "Excel" => ("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "xlsx"),
                _ => ("application/octet-stream", "txt")
            };

            var fileName = $"audit_logs_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{fileExtension}";

            // Registrar exportación
            await _auditService.LogSuccessAsync(
                currentUserId,
                "audit.export_logs",
                "audit_system",
                GetClientIpAddress(),
                resourceId: null,
                details: $"Exported audit logs in {request.Format} format",
                cancellationToken: cancellationToken);

            return File(exportStream, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting audit logs");
            return StatusCode(500, new ErrorResponse("internal_error", "Error exporting audit logs"));
        }
    }

    /// <summary>
    /// Limpia logs de auditoría antiguos según la política de retención
    /// </summary>
    [HttpPost("cleanup")]
    [RequirePermission(Permissions.System.Maintenance)]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CleanupOldLogs(CancellationToken cancellationToken, [FromQuery] int retentionDays = 365)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} initiating audit logs cleanup with retention of {RetentionDays} days", 
                currentUserId, retentionDays);

            // Validar días de retención
            retentionDays = Math.Max(30, Math.Min(retentionDays, 3650)); // Entre 30 días y 10 años

            var deletedCount = await _auditService.CleanupOldLogsAsync(retentionDays, cancellationToken);

            // Registrar limpieza
            await _auditService.LogSuccessAsync(
                currentUserId,
                "audit.cleanup_logs",
                "audit_system",
                GetClientIpAddress(),
                details: $"Cleaned up {deletedCount} old audit logs (retention: {retentionDays} days)",
                cancellationToken: cancellationToken);

            return Ok(new
            {
                message = "Audit logs cleanup completed successfully",
                deletedCount,
                retentionDays
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up audit logs");
            return StatusCode(500, new ErrorResponse("internal_error", "Error cleaning up audit logs"));
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user token");
        }
        return userId;
    }

    private string GetClientIpAddress()
    {
        // Intentar obtener la IP real del cliente
        var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        var realIp = Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
    }

    private static AuditSeverity DetermineSeverity(AuditLog log)
    {
        // Determinar severidad basada en la acción y éxito
        if (!log.Success)
        {
            return log.Action.Contains("delete") || log.Action.Contains("role_change") 
                ? AuditSeverity.Critical 
                : AuditSeverity.Error;
        }

        return log.Action switch
        {
            var action when action.Contains("delete") || action.Contains("role_change") => AuditSeverity.Warning,
            var action when action.Contains("login") || action.Contains("logout") => AuditSeverity.Info,
            var action when action.Contains("create") || action.Contains("update") => AuditSeverity.Info,
            _ => AuditSeverity.Info
        };
    }

    private async Task<List<SecurityAlert>> GenerateSecurityAlerts(CancellationToken cancellationToken)
    {
        // Esta es una implementación básica de generación de alertas
        // En una implementación completa, esto debería analizar patrones en los logs
        var alerts = new List<SecurityAlert>();

        try
        {
            // Buscar múltiples intentos fallidos en las últimas 24 horas
            var failedLoginsFilter = new AuditLogFilter
            {
                Action = "auth.login",
                Success = false,
                FromDate = DateTime.UtcNow.AddHours(-24),
                ToDate = DateTime.UtcNow,
                PageSize = 100
            };

            var failedLogins = await _auditService.GetAuditLogsAsync(failedLoginsFilter, cancellationToken);
            
            // Agrupar por IP y contar intentos fallidos
            var failedAttemptsByIp = failedLogins.Items
                .GroupBy(log => log.IpAddress)
                .Where(group => group.Count() >= 5) // 5 o más intentos fallidos
                .ToList();

            foreach (var group in failedAttemptsByIp)
            {
                alerts.Add(new SecurityAlert
                {
                    Id = Guid.NewGuid(),
                    Type = SecurityAlertType.MultipleFailedAttempts,
                    Severity = AuditSeverity.Warning,
                    Message = $"Multiple failed login attempts from IP {group.Key}",
                    IpAddress = group.Key,
                    EventCount = group.Count(),
                    FirstEventUtc = group.Min(log => log.TimestampUtc),
                    LastEventUtc = group.Max(log => log.TimestampUtc)
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating security alerts");
        }

        return alerts;
    }
}