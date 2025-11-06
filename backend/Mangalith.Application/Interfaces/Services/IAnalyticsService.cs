using Mangalith.Application.Common.Models;

namespace Mangalith.Application.Interfaces.Services;

public interface IAnalyticsService
{
    Task<ModerationAnalytics> GetModerationAnalyticsAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
    Task<PublicationMetrics> GetPublicationMetricsAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
    Task<List<ModeratorPerformance>> GetModeratorPerformanceAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
    Task<List<ContentTrend>> GetContentTrendsAsync(int days = 30, CancellationToken cancellationToken = default);
    Task<List<Common.Models.SystemAlert>> GetSystemAlertsAsync(bool includeResolved = false, CancellationToken cancellationToken = default);
    Task<byte[]> ExportAnalyticsReportAsync(DateTime fromDate, DateTime toDate, string format = "csv", CancellationToken cancellationToken = default);
    Task CheckAndCreateAlertsAsync(CancellationToken cancellationToken = default);
}