using System.Text;
using System.Text.Json;
using Mangalith.Application.Common.Models;
using Mangalith.Application.Interfaces.Repositories;
using Mangalith.Application.Interfaces.Services;
using Mangalith.Domain.Entities;
using Mangalith.Domain.Enums;

namespace Mangalith.Application.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly IPublicationRepository _publicationRepository;
    private readonly IModerationActionRepository _moderationActionRepository;
    private readonly IContentReportRepository _contentReportRepository;
    private readonly ISystemAlertRepository _systemAlertRepository;
    private readonly IUserRepository _userRepository;

    public AnalyticsService(
        IPublicationRepository publicationRepository,
        IModerationActionRepository moderationActionRepository,
        IContentReportRepository contentReportRepository,
        ISystemAlertRepository systemAlertRepository,
        IUserRepository userRepository)
    {
        _publicationRepository = publicationRepository;
        _moderationActionRepository = moderationActionRepository;
        _contentReportRepository = contentReportRepository;
        _systemAlertRepository = systemAlertRepository;
        _userRepository = userRepository;
    }

    public async Task<ModerationAnalytics> GetModerationAnalyticsAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var analytics = new ModerationAnalytics();

        // Get basic metrics
        analytics.Metrics = await GetModerationMetricsAsync(fromDate, toDate, cancellationToken);
        
        // Get moderator performance
        analytics.ModeratorPerformances = await GetModeratorPerformanceAsync(fromDate, toDate, cancellationToken);
        
        // Get content trends
        analytics.ContentTrends = await GetContentTrendsAsync(30, cancellationToken);
        
        // Get system alerts
        analytics.SystemAlerts = await GetSystemAlertsAsync(false, cancellationToken);

        return analytics;
    }

    public async Task<PublicationMetrics> GetPublicationMetricsAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var metrics = new PublicationMetrics();
        
        var now = DateTime.UtcNow;
        var today = now.Date;
        var weekStart = today.AddDays(-(int)today.DayOfWeek);
        var monthStart = new DateTime(today.Year, today.Month, 1);

        metrics.TotalSubmissions = await _publicationRepository.GetSubmissionCountAsync(fromDate, toDate, cancellationToken);
        metrics.SubmissionsToday = await _publicationRepository.GetSubmissionCountAsync(today, today.AddDays(1), cancellationToken);
        metrics.SubmissionsThisWeek = await _publicationRepository.GetSubmissionCountAsync(weekStart, now, cancellationToken);
        metrics.SubmissionsThisMonth = await _publicationRepository.GetSubmissionCountAsync(monthStart, now, cancellationToken);
        
        metrics.AverageProcessingTimeHours = await _publicationRepository.GetAverageReviewTimeAsync(fromDate, toDate, cancellationToken);
        metrics.ContentRatingDistribution = await _publicationRepository.GetContentRatingDistributionAsync(fromDate, toDate, cancellationToken);
        metrics.TopCreators = await _publicationRepository.GetTopCreatorsAsync(10, fromDate, toDate, cancellationToken);
        metrics.Trends = await _publicationRepository.GetPublicationTrendsAsync(30, cancellationToken);

        return metrics;
    }

    public async Task<List<ModeratorPerformance>> GetModeratorPerformanceAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        return await _moderationActionRepository.GetModeratorPerformanceAsync(fromDate, toDate, cancellationToken);
    }

    public async Task<List<ContentTrend>> GetContentTrendsAsync(int days = 30, CancellationToken cancellationToken = default)
    {
        var trends = new List<ContentTrend>();
        var endDate = DateTime.UtcNow;
        var startDate = endDate.AddDays(-days);
        var previousStartDate = startDate.AddDays(-days);

        // Publication trends
        var currentSubmissions = await _publicationRepository.GetSubmissionCountAsync(startDate, endDate, cancellationToken);
        var previousSubmissions = await _publicationRepository.GetSubmissionCountAsync(previousStartDate, startDate, cancellationToken);
        
        var submissionChange = previousSubmissions > 0 ? ((double)(currentSubmissions - previousSubmissions) / previousSubmissions) * 100 : 0;
        
        trends.Add(new ContentTrend
        {
            Category = "Submissions",
            Description = "Total publication submissions",
            Count = currentSubmissions,
            PercentageChange = submissionChange,
            Direction = submissionChange > 5 ? TrendDirection.Up : submissionChange < -5 ? TrendDirection.Down : TrendDirection.Stable,
            PeriodStart = startDate,
            PeriodEnd = endDate
        });

        // Report trends
        var currentReports = await _contentReportRepository.GetReportCountAsync(startDate, endDate, cancellationToken);
        var previousReports = await _contentReportRepository.GetReportCountAsync(previousStartDate, startDate, cancellationToken);
        
        var reportChange = previousReports > 0 ? ((double)(currentReports - previousReports) / previousReports) * 100 : 0;
        
        trends.Add(new ContentTrend
        {
            Category = "Reports",
            Description = "Content reports submitted",
            Count = currentReports,
            PercentageChange = reportChange,
            Direction = reportChange > 10 ? TrendDirection.Up : reportChange < -10 ? TrendDirection.Down : TrendDirection.Stable,
            PeriodStart = startDate,
            PeriodEnd = endDate
        });

        return trends;
    }

    public async Task<List<Common.Models.SystemAlert>> GetSystemAlertsAsync(bool includeResolved = false, CancellationToken cancellationToken = default)
    {
        var domainAlerts = await _systemAlertRepository.GetAllAlertsAsync(includeResolved, cancellationToken);
        
        return domainAlerts.Select(da => new Common.Models.SystemAlert
        {
            Id = da.Id,
            Type = da.Type,
            Severity = da.Severity,
            Title = da.Title,
            Description = da.Description,
            Metadata = string.IsNullOrEmpty(da.Metadata) ? 
                new Dictionary<string, object>() : 
                JsonSerializer.Deserialize<Dictionary<string, object>>(da.Metadata) ?? new Dictionary<string, object>(),
            CreatedAt = da.CreatedAt,
            IsResolved = da.IsResolved
        }).ToList();
    }

    public async Task<byte[]> ExportAnalyticsReportAsync(DateTime fromDate, DateTime toDate, string format = "csv", CancellationToken cancellationToken = default)
    {
        var analytics = await GetModerationAnalyticsAsync(fromDate, toDate, cancellationToken);
        var publicationMetrics = await GetPublicationMetricsAsync(fromDate, toDate, cancellationToken);

        if (format.ToLower() == "csv")
        {
            return GenerateCsvReport(analytics, publicationMetrics, fromDate, toDate);
        }

        throw new NotSupportedException($"Export format '{format}' is not supported");
    }

    public async Task CheckAndCreateAlertsAsync(CancellationToken cancellationToken = default)
    {
        var alerts = new List<Common.Models.SystemAlert>();

        // Check queue backlog
        var queueCount = await _publicationRepository.GetCountByStatusAsync(PublicationStatus.InReview, cancellationToken);
        if (queueCount > 50)
        {
            alerts.Add(new Common.Models.SystemAlert
            {
                Id = Guid.NewGuid(),
                Type = AlertType.QueueBacklog,
                Severity = queueCount > 100 ? AlertSeverity.High : AlertSeverity.Medium,
                Title = "Moderation Queue Backlog",
                Description = $"There are {queueCount} publications waiting for review",
                Metadata = new Dictionary<string, object> { { "QueueCount", queueCount } },
                CreatedAt = DateTime.UtcNow
            });
        }

        // Check for unusual activity patterns
        var recentReports = await _contentReportRepository.GetReportCountAsync(DateTime.UtcNow.AddHours(-24), DateTime.UtcNow, cancellationToken);
        if (recentReports > 20)
        {
            alerts.Add(new Common.Models.SystemAlert
            {
                Id = Guid.NewGuid(),
                Type = AlertType.UnusualActivity,
                Severity = AlertSeverity.Medium,
                Title = "High Report Volume",
                Description = $"Received {recentReports} reports in the last 24 hours",
                Metadata = new Dictionary<string, object> { { "ReportCount", recentReports } },
                CreatedAt = DateTime.UtcNow
            });
        }

        // Check average review time
        var avgReviewTime = await _publicationRepository.GetAverageReviewTimeAsync(DateTime.UtcNow.AddDays(-7), DateTime.UtcNow, cancellationToken);
        if (avgReviewTime > 72) // More than 3 days
        {
            alerts.Add(new Common.Models.SystemAlert
            {
                Id = Guid.NewGuid(),
                Type = AlertType.PerformanceIssue,
                Severity = AlertSeverity.Medium,
                Title = "Slow Review Times",
                Description = $"Average review time is {avgReviewTime:F1} hours",
                Metadata = new Dictionary<string, object> { { "AverageHours", avgReviewTime } },
                CreatedAt = DateTime.UtcNow
            });
        }

        // Save new alerts
        foreach (var alert in alerts)
        {
            var systemAlert = new Domain.Entities.SystemAlert(
                alert.Type,
                alert.Severity,
                alert.Title,
                alert.Description,
                JsonSerializer.Serialize(alert.Metadata)
            );
            await _systemAlertRepository.CreateAsync(systemAlert, cancellationToken);
        }
    }

    private async Task<ModerationMetrics> GetModerationMetricsAsync(DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken)
    {
        var metrics = new ModerationMetrics();

        metrics.StatusDistribution = await _publicationRepository.GetStatusDistributionAsync(fromDate, toDate, cancellationToken);
        metrics.ReportCategoryDistribution = await _contentReportRepository.GetCategoryDistributionAsync(fromDate, toDate, cancellationToken);
        
        metrics.TotalPublications = metrics.StatusDistribution.Values.Sum();
        metrics.PublicationsInReview = metrics.StatusDistribution.GetValueOrDefault(PublicationStatus.InReview, 0);
        metrics.PublicationsApproved = metrics.StatusDistribution.GetValueOrDefault(PublicationStatus.Published, 0);
        metrics.PublicationsRejected = metrics.StatusDistribution.GetValueOrDefault(PublicationStatus.Rejected, 0);
        metrics.PublicationsArchived = metrics.StatusDistribution.GetValueOrDefault(PublicationStatus.Archived, 0);

        metrics.PendingReports = await _contentReportRepository.GetCountByStatusAsync(ContentReportStatus.Pending, cancellationToken);
        metrics.ResolvedReports = await _contentReportRepository.GetCountByStatusAsync(ContentReportStatus.Resolved, cancellationToken);

        metrics.AverageReviewTimeHours = await _publicationRepository.GetAverageReviewTimeAsync(fromDate, toDate, cancellationToken);
        
        var totalProcessed = metrics.PublicationsApproved + metrics.PublicationsRejected;
        metrics.ApprovalRate = totalProcessed > 0 ? (double)metrics.PublicationsApproved / totalProcessed * 100 : 0;

        return metrics;
    }

    private byte[] GenerateCsvReport(ModerationAnalytics analytics, PublicationMetrics publicationMetrics, DateTime fromDate, DateTime toDate)
    {
        var csv = new StringBuilder();
        
        // Header
        csv.AppendLine($"Mangalith Analytics Report - {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}");
        csv.AppendLine($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        csv.AppendLine();

        // Moderation Metrics
        csv.AppendLine("MODERATION METRICS");
        csv.AppendLine("Metric,Value");
        csv.AppendLine($"Total Publications,{analytics.Metrics.TotalPublications}");
        csv.AppendLine($"In Review,{analytics.Metrics.PublicationsInReview}");
        csv.AppendLine($"Approved,{analytics.Metrics.PublicationsApproved}");
        csv.AppendLine($"Rejected,{analytics.Metrics.PublicationsRejected}");
        csv.AppendLine($"Archived,{analytics.Metrics.PublicationsArchived}");
        csv.AppendLine($"Pending Reports,{analytics.Metrics.PendingReports}");
        csv.AppendLine($"Resolved Reports,{analytics.Metrics.ResolvedReports}");
        csv.AppendLine($"Average Review Time (hours),{analytics.Metrics.AverageReviewTimeHours:F2}");
        csv.AppendLine($"Approval Rate (%),{analytics.Metrics.ApprovalRate:F2}");
        csv.AppendLine();

        // Moderator Performance
        csv.AppendLine("MODERATOR PERFORMANCE");
        csv.AppendLine("Moderator,Actions Completed,Approvals,Rejections,Reports Reviewed,Avg Review Time (hours),Approval Rate (%)");
        foreach (var moderator in analytics.ModeratorPerformances)
        {
            csv.AppendLine($"{moderator.ModeratorName},{moderator.ActionsCompleted},{moderator.ApprovalsCount},{moderator.RejectionsCount},{moderator.ReportsReviewed},{moderator.AverageReviewTimeHours:F2},{moderator.ApprovalRate:F2}");
        }
        csv.AppendLine();

        // Content Trends
        csv.AppendLine("CONTENT TRENDS");
        csv.AppendLine("Category,Description,Count,Change (%),Direction");
        foreach (var trend in analytics.ContentTrends)
        {
            csv.AppendLine($"{trend.Category},{trend.Description},{trend.Count},{trend.PercentageChange:F2},{trend.Direction}");
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }
}