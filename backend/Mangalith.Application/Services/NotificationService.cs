using Microsoft.Extensions.Logging;
using Mangalith.Application.Interfaces.Services;
using Mangalith.Application.Common.Exceptions;
using Mangalith.Domain.Entities;
using Mangalith.Domain.Enums;

namespace Mangalith.Application.Services;

/// <summary>
/// Servicio de notificaciones que proporciona logs y eventos de aplicación
/// En la producción, esto debería integrar con un servicio real de emails/push notifications
/// </summary>
public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private const int MaxRetryAttempts = 3;
    private static readonly TimeSpan[] RetryDelays = { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(15) };

    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger;
    }

    public async Task NotifyPublicationStatusChangedAsync(Publication publication, PublicationStatus previousStatus, Guid notifiedUserId, string? message = null, CancellationToken cancellationToken = default)
    {
        var logMessage = $"Publication {publication.Id} status changed from {previousStatus} to {publication.Status}. " +
                         $"Notifying user {notifiedUserId}. Message: {message ?? "No additional message"}";
        
        await ExecuteWithRetryAsync(
            async () =>
            {
                _logger.LogInformation("NOTIFICATION: {LogMessage}", logMessage);
                // TODO: Implementar envío real de notificaciones (email, push, etc.)
                await Task.CompletedTask;
            },
            "PublicationStatusChanged",
            notifiedUserId.ToString(),
            cancellationToken);
    }

    public async Task NotifyPublicationSubmittedAsync(Publication publication, CancellationToken cancellationToken = default)
    {
        var logMessage = $"Publication {publication.Id} submitted for review. Manga: {publication.Manga?.Title}. " +
                         $"Creator: {publication.CreatedByUser?.Email}. Submitted at: {publication.SubmittedAtUtc}";
        
        await ExecuteWithRetryAsync(
            async () =>
            {
                _logger.LogInformation("NOTIFICATION: {LogMessage}", logMessage);
                // TODO: Notificar a moderadores disponibles
                await Task.CompletedTask;
            },
            "PublicationSubmitted",
            "moderators",
            cancellationToken);
    }

    public async Task NotifyPublicationApprovedAsync(Publication publication, string? moderatorComments = null, CancellationToken cancellationToken = default)
    {
        var logMessage = $"Publication {publication.Id} approved! Manga: {publication.Manga?.Title}. " +
                         $"Content Rating: {publication.ContentRating}. NSFW: {publication.IsNsfw}. " +
                         $"Moderator Comments: {moderatorComments ?? "None"}";
        
        await ExecuteWithRetryAsync(
            async () =>
            {
                _logger.LogInformation("NOTIFICATION: Publication Approved - {LogMessage}", logMessage);
                // TODO: Enviar email de aprobación al creador
                await Task.CompletedTask;
            },
            "PublicationApproved",
            publication.CreatedByUserId.ToString(),
            cancellationToken);
    }

    public async Task NotifyPublicationRejectedAsync(Publication publication, string rejectionReason, string? moderatorComments = null, CancellationToken cancellationToken = default)
    {
        var logMessage = $"Publication {publication.Id} rejected. Manga: {publication.Manga?.Title}. " +
                         $"Reason: {rejectionReason}. Moderator Comments: {moderatorComments ?? "None"}";
        
        await ExecuteWithRetryAsync(
            async () =>
            {
                _logger.LogWarning("NOTIFICATION: Publication Rejected - {LogMessage}", logMessage);
                // TODO: Enviar email de rechazo con detalles al creador
                await Task.CompletedTask;
            },
            "PublicationRejected",
            publication.CreatedByUserId.ToString(),
            cancellationToken);
    }

    public async Task NotifyPublicationNeedsRevisionAsync(Publication publication, string? comments = null, CancellationToken cancellationToken = default)
    {
        var logMessage = $"Publication {publication.Id} needs revision. Manga: {publication.Manga?.Title}. " +
                         $"Moderator Comments: {comments ?? "No comments provided"}";
        
        await ExecuteWithRetryAsync(
            async () =>
            {
                _logger.LogInformation("NOTIFICATION: Revision Needed - {LogMessage}", logMessage);
                // TODO: Enviar email con instrucciones de revisión
                await Task.CompletedTask;
            },
            "PublicationNeedsRevision",
            publication.CreatedByUserId.ToString(),
            cancellationToken);
    }

    public async Task NotifyContentReportedAsync(Publication publication, ContentReport report, CancellationToken cancellationToken = default)
    {
        var logMessage = $"Content report created. Report ID: {report.Id}. Publication: {publication.Id}. " +
                         $"Category: {report.Category}. Reporter: {report.ReportedByUser?.Email}. " +
                         $"Description: {report.Description}";
        
        await ExecuteWithRetryAsync(
            async () =>
            {
                _logger.LogWarning("NOTIFICATION: Content Reported - {LogMessage}", logMessage);
                // TODO: Alertar a moderadores sobre nuevo reporte
                await Task.CompletedTask;
            },
            "ContentReported",
            "moderators",
            cancellationToken);
    }

    public async Task NotifyReportResolvedAsync(ContentReport report, ContentReportStatus newStatus, string? response = null, CancellationToken cancellationToken = default)
    {
        var logMessage = $"Report {report.Id} status changed to {newStatus}. " +
                         $"Publication: {report.PublicationId}. Moderator Response: {response ?? "No response provided"}";
        
        await ExecuteWithRetryAsync(
            async () =>
            {
                _logger.LogInformation("NOTIFICATION: Report Resolved - {LogMessage}", logMessage);
                // TODO: Notificar al reportero sobre la resolución
                await Task.CompletedTask;
            },
            "ReportResolved",
            report.ReportedByUserId.ToString(),
            cancellationToken);
    }

    public async Task NotifyModeratorsAboutPendingReportsAsync(int pendingReportCount, CancellationToken cancellationToken = default)
    {
        var logMessage = $"Alert: {pendingReportCount} pending reports waiting for review";
        
        await ExecuteWithRetryAsync(
            async () =>
            {
                _logger.LogInformation("NOTIFICATION: Moderator Alert - {LogMessage}", logMessage);
                // TODO: Enviar notificaciones a moderadores disponibles
                await Task.CompletedTask;
            },
            "ModeratorsAlert",
            "moderators",
            cancellationToken);
    }

    /// <summary>
    /// Executes notification delivery with retry logic
    /// </summary>
    private async Task ExecuteWithRetryAsync(Func<Task> operation, string notificationType, string recipient, CancellationToken cancellationToken)
    {
        Exception? lastException = null;

        for (int attempt = 0; attempt < MaxRetryAttempts; attempt++)
        {
            try
            {
                await operation();
                
                // Log successful delivery after retry
                if (attempt > 0)
                {
                    _logger.LogInformation("Notification delivery succeeded after {Attempt} retries. Type: {NotificationType}, Recipient: {Recipient}", 
                        attempt, notificationType, recipient);
                }
                
                return;
            }
            catch (Exception ex) when (attempt < MaxRetryAttempts - 1)
            {
                lastException = ex;
                var delay = RetryDelays[attempt];
                
                _logger.LogWarning(ex, "Notification delivery failed (attempt {Attempt}/{MaxAttempts}). Type: {NotificationType}, Recipient: {Recipient}. Retrying in {Delay}ms", 
                    attempt + 1, MaxRetryAttempts, notificationType, recipient, delay.TotalMilliseconds);
                
                await Task.Delay(delay, cancellationToken);
            }
            catch (Exception ex)
            {
                lastException = ex;
                break;
            }
        }

        // All retries failed
        _logger.LogError(lastException, "Notification delivery failed after {MaxAttempts} attempts. Type: {NotificationType}, Recipient: {Recipient}", 
            MaxRetryAttempts, notificationType, recipient);
        
        throw new NotificationDeliveryException(notificationType, recipient, 
            $"Failed to deliver notification after {MaxRetryAttempts} attempts", lastException!);
    }
}
