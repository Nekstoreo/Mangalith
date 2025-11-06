namespace Mangalith.Tests.TestHelpers;

public interface IMockQuotaService
{
    Task<bool> CanUploadFileAsync(Guid userId, long fileSize, CancellationToken cancellationToken = default);
    Task<MockQuotaUsageReport> GetQuotaUsageReportAsync(Guid userId, CancellationToken cancellationToken = default);
    Task TrackFileUploadAsync(Guid userId, long fileSize, CancellationToken cancellationToken = default);
    Task TrackFileDeleteAsync(Guid userId, long fileSize, CancellationToken cancellationToken = default);
}

public class MockQuotaUsageReport
{
    public bool HasExceededAnyLimit { get; set; }
    public double StorageUsagePercentage { get; set; }
    public long StorageUsedBytes { get; set; }
    public long StorageQuotaBytes { get; set; }
    public int FilesUploadedToday { get; set; }
    public int DailyUploadLimit { get; set; }
}