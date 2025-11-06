using Mangalith.Application.Common.Exceptions;
using Mangalith.Application.Common.Models;
using Mangalith.Application.Interfaces.Repositories;
using Mangalith.Application.Interfaces.Services;
using Mangalith.Domain.Entities;
using Mangalith.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Mangalith.Application.Services;

public class ContentReportService : IContentReportService
{
    private readonly IContentReportRepository _contentReportRepository;
    private readonly IPublicationRepository _publicationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPublicationValidationService _validationService;
    private readonly ILogger<ContentReportService> _logger;

    public ContentReportService(
        IContentReportRepository contentReportRepository,
        IPublicationRepository publicationRepository,
        IUserRepository userRepository,
        IPublicationValidationService validationService,
        ILogger<ContentReportService> logger)
    {
        _contentReportRepository = contentReportRepository;
        _publicationRepository = publicationRepository;
        _userRepository = userRepository;
        _validationService = validationService;
        _logger = logger;
    }

    public async Task<ContentReport> CreateReportAsync(Guid publicationId, Guid reportedByUserId, ContentReportCategory category, string description, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating content report for publication {PublicationId} by user {UserId}, category: {Category}", 
            publicationId, reportedByUserId, category);

        // Validate content report parameters
        await _validationService.ValidateContentReportAsync(publicationId, reportedByUserId, description, cancellationToken);

        var publication = await _publicationRepository.GetByIdAsync(publicationId, cancellationToken)
            ?? throw new NotFoundException($"Publication with ID {publicationId} not found");

        // Validar que el usuario no sea el creador (no puede reportar su propio contenido)
        if (publication.CreatedByUserId == reportedByUserId)
        {
            throw new ContentReportValidationException("SELF_REPORT_NOT_ALLOWED", "You cannot report your own content");
        }

        // Validate category
        if (!Enum.IsDefined(typeof(ContentReportCategory), category))
        {
            throw new ContentReportValidationException("INVALID_CATEGORY", "Invalid report category specified");
        }

        // Check for duplicate reports from same user
        var existingReports = await _contentReportRepository.GetByPublicationAndUserAsync(publicationId, reportedByUserId, cancellationToken);
        if (existingReports.Any(r => r.Status == ContentReportStatus.Pending))
        {
            throw new ContentReportValidationException("DUPLICATE_REPORT", "You have already reported this content");
        }

        // Crear reporte
        var report = new ContentReport(publicationId, reportedByUserId, category, description);
        var createdReport = await _contentReportRepository.CreateAsync(report, cancellationToken);
        
        // Si la publicación no está bajo revisión, marcarla como tal si es la primera o segunda reporte
        var reportCount = await _contentReportRepository.GetCountByPublicationIdAsync(publicationId, cancellationToken);
        if (reportCount >= 2 && publication.Status == PublicationStatus.Published)
        {
            // Marcar como bajo revisión después de múltiples reportes
            publication.MarkUnderReview();
            await _publicationRepository.UpdateAsync(publication, cancellationToken);
            _logger.LogWarning("Publication {PublicationId} marked under review due to multiple reports", publicationId);
        }

        _logger.LogInformation("Content report {ReportId} created successfully for publication {PublicationId}", 
            createdReport.Id, publicationId);
        
        return createdReport;
    }

    public async Task<ContentReport> ReviewReportAsync(Guid reportId, Guid moderatorId, ContentReportStatus status, string? response = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Reviewing report {ReportId} by moderator {ModeratorId}, new status: {Status}", 
            reportId, moderatorId, status);

        var report = await _contentReportRepository.GetByIdAsync(reportId, cancellationToken)
            ?? throw new NotFoundException($"Report with ID {reportId} not found");

        // Validate moderator permissions
        await _validationService.ValidateModeratorPermissionsAsync(moderatorId, cancellationToken);

        // Validar que el reporte esté en estado Pending o UnderReview
        if (report.Status != ContentReportStatus.Pending && report.Status != ContentReportStatus.UnderReview)
        {
            throw new ContentReportValidationException("INVALID_REPORT_STATUS", $"Cannot review report with status {report.Status}");
        }

        // Validate new status
        if (!Enum.IsDefined(typeof(ContentReportStatus), status))
        {
            throw new ContentReportValidationException("INVALID_STATUS", "Invalid report status specified");
        }

        // Validate response for resolved/dismissed reports
        if ((status == ContentReportStatus.Resolved || status == ContentReportStatus.Dismissed) && string.IsNullOrWhiteSpace(response))
        {
            throw new ContentReportValidationException("MISSING_RESPONSE", "Response is required when resolving or dismissing reports");
        }

        if (!string.IsNullOrEmpty(response) && response.Length > 1000)
        {
            throw new ContentReportValidationException("RESPONSE_TOO_LONG", "Response cannot exceed 1000 characters");
        }

        // Marcar según el estatus
        if (status == ContentReportStatus.Resolved)
        {
            report.Resolve(moderatorId, response ?? "Report resolved");
        }
        else if (status == ContentReportStatus.Dismissed)
        {
            report.Dismiss(moderatorId, response ?? "Report dismissed");
        }
        else if (status == ContentReportStatus.UnderReview)
        {
            report.MarkUnderReview(moderatorId);
        }

        var updatedReport = await _contentReportRepository.UpdateAsync(report, cancellationToken);
        
        _logger.LogInformation("Report {ReportId} reviewed successfully, status changed to {Status}", reportId, status);
        
        return updatedReport;
    }

    public async Task<PagedResult<ContentReport>> GetPendingReportsAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _contentReportRepository.GetPendingAsync(page, pageSize, cancellationToken);
    }

    public async Task<PagedResult<ContentReport>> GetReportsByPublicationAsync(Guid publicationId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        // Verificar que la publicación existe
        var publication = await _publicationRepository.GetByIdAsync(publicationId, cancellationToken)
            ?? throw new NotFoundException($"Publication with ID {publicationId} not found");

        return await _contentReportRepository.GetByPublicationIdAsync(publicationId, page, pageSize, cancellationToken);
    }

    public async Task<int> GetReportCountByPublicationAsync(Guid publicationId, CancellationToken cancellationToken = default)
    {
        // Verificar que la publicación existe
        var publication = await _publicationRepository.GetByIdAsync(publicationId, cancellationToken)
            ?? throw new NotFoundException($"Publication with ID {publicationId} not found");

        return await _contentReportRepository.GetCountByPublicationIdAsync(publicationId, cancellationToken);
    }

    public async Task<PagedResult<ContentReport>> GetUserReportsAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        // Verificar que el usuario existe
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException($"User with ID {userId} not found");

        return await _contentReportRepository.GetByUserIdAsync(userId, page, pageSize, cancellationToken);
    }
}
