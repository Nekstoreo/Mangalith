namespace Mangalith.Application.Common.Models;

/// <summary>
/// Statistics for moderation activities and performance
/// </summary>
public class ModerationStatistics
{
    public int TotalInReview { get; set; }
    public int TotalNeedsRevision { get; set; }
    public int TotalPublished { get; set; }
    public int TotalRejected { get; set; }
    public double ApprovalRate { get; set; }
    public int TotalActionsInPeriod { get; set; }
    public int ApprovalsInPeriod { get; set; }
    public int RejectionsInPeriod { get; set; }
    public int RevisionsInPeriod { get; set; }
}