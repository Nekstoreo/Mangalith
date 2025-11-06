using Mangalith.Domain.Enums;

namespace Mangalith.Application.Common.Models;

public class ModerationAnalytics
{
    public ModerationMetrics Metrics { get; set; } = new();
    public List<ModeratorPerformance> ModeratorPerformances { get; set; } = new();
    public List<ContentTrend> ContentTrends { get; set; } = new();
    public List<SystemAlert> SystemAlerts { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class ModerationMetrics
{
    public int TotalPublications { get; set; }
    public int PublicationsInReview { get; set; }
    public int PublicationsApproved { get; set; }
    public int PublicationsRejected { get; set; }
    public int PublicationsArchived { get; set; }
    public int PendingReports { get; set; }
    public int ResolvedReports { get; set; }
    public double AverageReviewTimeHours { get; set; }
    public double ApprovalRate { get; set; }
    public Dictionary<PublicationStatus, int> StatusDistribution { get; set; } = new();
    public Dictionary<ContentReportCategory, int> ReportCategoryDistribution { get; set; } = new();
}

public class PublicationMetrics
{
    public int TotalSubmissions { get; set; }
    public int SubmissionsToday { get; set; }
    public int SubmissionsThisWeek { get; set; }
    public int SubmissionsThisMonth { get; set; }
    public double AverageProcessingTimeHours { get; set; }
    public Dictionary<ContentRating, int> ContentRatingDistribution { get; set; } = new();
    public Dictionary<string, int> TopCreators { get; set; } = new();
    public List<PublicationTrend> Trends { get; set; } = new();
}

public class ModeratorPerformance
{
    public Guid ModeratorId { get; set; }
    public string ModeratorName { get; set; } = string.Empty;
    public int ActionsCompleted { get; set; }
    public int ApprovalsCount { get; set; }
    public int RejectionsCount { get; set; }
    public int ReportsReviewed { get; set; }
    public double AverageReviewTimeHours { get; set; }
    public double ApprovalRate { get; set; }
    public DateTime LastActiveAt { get; set; }
    public int ActionsLast7Days { get; set; }
    public int ActionsLast30Days { get; set; }
}

public class ContentTrend
{
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Count { get; set; }
    public double PercentageChange { get; set; }
    public TrendDirection Direction { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
}

public class SystemAlert
{
    public Guid Id { get; set; }
    public AlertType Type { get; set; }
    public AlertSeverity Severity { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public bool IsResolved { get; set; }
}

public class PublicationTrend
{
    public DateTime Date { get; set; }
    public int Submissions { get; set; }
    public int Approvals { get; set; }
    public int Rejections { get; set; }
    public double AverageReviewTime { get; set; }
}

