using Mangalith.Domain.Entities;
using Mangalith.Domain.Enums;

namespace Mangalith.Application.Interfaces.Services;

public interface INotificationService
{
    /// <summary>
    /// Notifica a un usuario sobre un cambio en el estado de su publicación
    /// </summary>
    Task NotifyPublicationStatusChangedAsync(Publication publication, PublicationStatus previousStatus, Guid notifiedUserId, string? message = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifica a moderadores cuando una publicación es sometida para revisión
    /// </summary>
    Task NotifyPublicationSubmittedAsync(Publication publication, CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifica a un usuario cuando su publicación es aprobada
    /// </summary>
    Task NotifyPublicationApprovedAsync(Publication publication, string? moderatorComments = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifica a un usuario cuando su publicación es rechazada
    /// </summary>
    Task NotifyPublicationRejectedAsync(Publication publication, string rejectionReason, string? moderatorComments = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifica a un usuario cuando su publicación requiere revisión
    /// </summary>
    Task NotifyPublicationNeedsRevisionAsync(Publication publication, string? comments = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifica a un usuario cuando reportan su contenido
    /// </summary>
    Task NotifyContentReportedAsync(Publication publication, ContentReport report, CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifica al reportero sobre la resolución de su reporte
    /// </summary>
    Task NotifyReportResolvedAsync(ContentReport report, ContentReportStatus newStatus, string? response = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifica a moderadores sobre reportes pendientes
    /// </summary>
    Task NotifyModeratorsAboutPendingReportsAsync(int pendingReportCount, CancellationToken cancellationToken = default);
}
