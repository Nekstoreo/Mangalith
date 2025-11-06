using FluentAssertions;
using Mangalith.Domain.Constants;
using Mangalith.Domain.Entities;
using Mangalith.Tests.TestHelpers;

namespace Mangalith.Tests.Domain.Entities;

public class UserQuotaTests
{
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public void Constructor_WithValidUserId_ShouldCreateUserQuotaWithCorrectProperties()
    {
        // Act
        var userQuota = new UserQuota(_userId);

        // Assert
        userQuota.Id.Should().NotBe(Guid.Empty);
        userQuota.UserId.Should().Be(_userId);
        userQuota.StorageUsedBytes.Should().Be(0);
        userQuota.FilesUploadedToday.Should().Be(0);
        userQuota.MangasCreated.Should().Be(0);
        userQuota.LastResetDate.Should().Be(DateTime.UtcNow.Date);
        userQuota.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        userQuota.UpdatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(1024)]
    [InlineData(1048576)] // 1MB
    [InlineData(1073741824)] // 1GB
    public void AddStorageUsage_WithValidBytes_ShouldIncreaseStorageAndUpdateTimestamp(long bytes)
    {
        // Arrange
        var userQuota = new UserQuota(_userId);
        var originalUpdatedAt = userQuota.UpdatedAtUtc;

        // Act
        userQuota.AddStorageUsage(bytes);

        // Assert
        userQuota.StorageUsedBytes.Should().Be(bytes);
        userQuota.UpdatedAtUtc.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void AddStorageUsage_WithNegativeBytes_ShouldThrowArgumentException()
    {
        // Arrange
        var userQuota = new UserQuota(_userId);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => userQuota.AddStorageUsage(-1024));
        exception.ParamName.Should().Be("bytes");
        exception.Message.Should().Contain("Storage bytes cannot be negative");
    }

    [Fact]
    public void AddStorageUsage_Multiple_ShouldAccumulate()
    {
        // Arrange
        var userQuota = new UserQuota(_userId);

        // Act
        userQuota.AddStorageUsage(1024);
        userQuota.AddStorageUsage(2048);
        userQuota.AddStorageUsage(512);

        // Assert
        userQuota.StorageUsedBytes.Should().Be(3584);
    }

    [Theory]
    [InlineData(1024, 512, 512)]
    [InlineData(1000, 1000, 0)]
    [InlineData(500, 1000, 0)] // Should not go below 0
    public void RemoveStorageUsage_WithValidBytes_ShouldDecreaseStorageCorrectly(long initial, long remove, long expected)
    {
        // Arrange
        var userQuota = new UserQuota(_userId);
        userQuota.AddStorageUsage(initial);
        var originalUpdatedAt = userQuota.UpdatedAtUtc;

        // Act
        userQuota.RemoveStorageUsage(remove);

        // Assert
        userQuota.StorageUsedBytes.Should().Be(expected);
        userQuota.UpdatedAtUtc.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void RemoveStorageUsage_WithNegativeBytes_ShouldThrowArgumentException()
    {
        // Arrange
        var userQuota = new UserQuota(_userId);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => userQuota.RemoveStorageUsage(-1024));
        exception.ParamName.Should().Be("bytes");
        exception.Message.Should().Contain("Storage bytes cannot be negative");
    }

    [Fact]
    public void IncrementFileUpload_ShouldIncreaseCounterAndUpdateTimestamp()
    {
        // Arrange
        var userQuota = new UserQuota(_userId);
        var originalUpdatedAt = userQuota.UpdatedAtUtc;

        // Act
        userQuota.IncrementFileUpload();

        // Assert
        userQuota.FilesUploadedToday.Should().Be(1);
        userQuota.UpdatedAtUtc.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void IncrementFileUpload_Multiple_ShouldAccumulate()
    {
        // Arrange
        var userQuota = new UserQuota(_userId);

        // Act
        userQuota.IncrementFileUpload();
        userQuota.IncrementFileUpload();
        userQuota.IncrementFileUpload();

        // Assert
        userQuota.FilesUploadedToday.Should().Be(3);
    }

    [Fact]
    public void IncrementMangaCreation_ShouldIncreaseCounterAndUpdateTimestamp()
    {
        // Arrange
        var userQuota = new UserQuota(_userId);
        var originalUpdatedAt = userQuota.UpdatedAtUtc;

        // Act
        userQuota.IncrementMangaCreation();

        // Assert
        userQuota.MangasCreated.Should().Be(1);
        userQuota.UpdatedAtUtc.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void DecrementMangaCreation_ShouldDecreaseCounterAndUpdateTimestamp()
    {
        // Arrange
        var userQuota = new UserQuota(_userId);
        userQuota.IncrementMangaCreation();
        userQuota.IncrementMangaCreation();
        var originalUpdatedAt = userQuota.UpdatedAtUtc;

        // Act
        userQuota.DecrementMangaCreation();

        // Assert
        userQuota.MangasCreated.Should().Be(1);
        userQuota.UpdatedAtUtc.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void DecrementMangaCreation_WhenZero_ShouldNotGoBelowZero()
    {
        // Arrange
        var userQuota = new UserQuota(_userId);

        // Act
        userQuota.DecrementMangaCreation();

        // Assert
        userQuota.MangasCreated.Should().Be(0);
    }

    [Fact]
    public void ResetDailyCountersIfNeeded_WhenSameDay_ShouldNotReset()
    {
        // Arrange
        var userQuota = new UserQuota(_userId);
        userQuota.IncrementFileUpload();
        userQuota.IncrementFileUpload();

        // Act
        userQuota.ResetDailyCountersIfNeeded();

        // Assert
        userQuota.FilesUploadedToday.Should().Be(2);
        userQuota.LastResetDate.Should().Be(DateTime.UtcNow.Date);
    }

    [Theory]
    [InlineData(UserRole.Uploader, 50.0)] // 5GB quota, 50% used
    [InlineData(UserRole.Moderator, 50.0)] // 20GB quota, 50% used
    public void GetStorageUsagePercentage_ShouldCalculateCorrectPercentage(UserRole role, double expectedPercentage)
    {
        // Arrange
        var userQuota = new UserQuota(_userId);
        var quota = QuotaLimits.GetStorageQuota(role);
        var usedBytes = (long)(quota * expectedPercentage / 100);
        userQuota.AddStorageUsage(usedBytes);

        // Act
        var percentage = userQuota.GetStorageUsagePercentage(role);

        // Assert
        percentage.Should().BeApproximately(expectedPercentage, 0.1);
    }

    [Theory]
    [InlineData(UserRole.Uploader, 4429185226L, true)] // >80% of 5GB
    [InlineData(UserRole.Uploader, 2684354560L, false)] // 50% of 5GB
    [InlineData(UserRole.Moderator, 17316868259L, true)] // >80% of 20GB
    public void IsNearStorageLimit_ShouldReturnCorrectResult(UserRole role, long usedBytes, bool expectedResult)
    {
        // Arrange
        var userQuota = new UserQuota(_userId);
        userQuota.AddStorageUsage(usedBytes);

        // Act
        var result = userQuota.IsNearStorageLimit(role);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(UserRole.Uploader, 5368709121L, true)] // 5GB + 1 byte
    [InlineData(UserRole.Uploader, 5368709120L, true)] // Exactly 5GB
    [InlineData(UserRole.Uploader, 5368709119L, false)] // 5GB - 1 byte
    public void HasExceededStorageQuota_ShouldReturnCorrectResult(UserRole role, long usedBytes, bool expectedResult)
    {
        // Arrange
        var userQuota = new UserQuota(_userId);
        userQuota.AddStorageUsage(usedBytes);

        // Act
        var result = userQuota.HasExceededStorageQuota(role);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(UserRole.Uploader, 10, true)] // Uploader limit is 10
    [InlineData(UserRole.Uploader, 9, false)]
    [InlineData(UserRole.Moderator, 50, true)] // Moderator limit is 50
    [InlineData(UserRole.Moderator, 49, false)]
    public void HasExceededDailyUploadLimit_ShouldReturnCorrectResult(UserRole role, int uploads, bool expectedResult)
    {
        // Arrange
        var userQuota = new UserQuota(_userId);
        for (int i = 0; i < uploads; i++)
        {
            userQuota.IncrementFileUpload();
        }

        // Act
        var result = userQuota.HasExceededDailyUploadLimit(role);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(UserRole.Uploader, 20, true)] // Uploader limit is 20
    [InlineData(UserRole.Uploader, 19, false)]
    [InlineData(UserRole.Moderator, 100, true)] // Moderator limit is 100
    [InlineData(UserRole.Moderator, 99, false)]
    public void HasExceededMangaCreationLimit_ShouldReturnCorrectResult(UserRole role, int mangasCreated, bool expectedResult)
    {
        // Arrange
        var userQuota = new UserQuota(_userId);
        for (int i = 0; i < mangasCreated; i++)
        {
            userQuota.IncrementMangaCreation();
        }

        // Act
        var result = userQuota.HasExceededMangaCreationLimit(role);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(UserRole.Uploader, 2684354560L, 2684354560L)] // 2.5GB used, 2.5GB remaining from 5GB
    [InlineData(UserRole.Uploader, 5368709120L, 0L)] // 5GB used, 0 remaining from 5GB
    public void GetRemainingStorageBytes_ShouldReturnCorrectAmount(UserRole role, long usedBytes, long expectedRemaining)
    {
        // Arrange
        var userQuota = new UserQuota(_userId);
        userQuota.AddStorageUsage(usedBytes);

        // Act
        var remaining = userQuota.GetRemainingStorageBytes(role);

        // Assert
        remaining.Should().Be(expectedRemaining);
    }

    [Theory]
    [InlineData(UserRole.Uploader, 5, 5)] // 5 uploaded, 5 remaining from 10
    [InlineData(UserRole.Uploader, 10, 0)] // 10 uploaded, 0 remaining from 10
    public void GetRemainingDailyUploads_ShouldReturnCorrectAmount(UserRole role, int uploaded, int expectedRemaining)
    {
        // Arrange
        var userQuota = new UserQuota(_userId);
        for (int i = 0; i < uploaded; i++)
        {
            userQuota.IncrementFileUpload();
        }

        // Act
        var remaining = userQuota.GetRemainingDailyUploads(role);

        // Assert
        remaining.Should().Be(expectedRemaining);
    }

    [Theory]
    [InlineData(UserRole.Uploader, 10, 10)] // 10 created, 10 remaining from 20
    [InlineData(UserRole.Uploader, 20, 0)] // 20 created, 0 remaining from 20
    public void GetRemainingMangaCreations_ShouldReturnCorrectAmount(UserRole role, int created, int expectedRemaining)
    {
        // Arrange
        var userQuota = new UserQuota(_userId);
        for (int i = 0; i < created; i++)
        {
            userQuota.IncrementMangaCreation();
        }

        // Act
        var remaining = userQuota.GetRemainingMangaCreations(role);

        // Assert
        remaining.Should().Be(expectedRemaining);
    }

    [Fact]
    public void GetRemainingMangaCreations_ForAdministrator_ShouldReturnMaxValue()
    {
        // Arrange
        var userQuota = new UserQuota(_userId);
        for (int i = 0; i < 1000; i++)
        {
            userQuota.IncrementMangaCreation();
        }

        // Act
        var remaining = userQuota.GetRemainingMangaCreations(UserRole.Administrator);

        // Assert
        remaining.Should().Be(int.MaxValue);
    }

    [Fact]
    public void UserQuota_WithBuilderPattern_ShouldCreateCorrectly()
    {
        // Arrange & Act
        var userQuota = TestDataBuilder.CreateUserQuota()
            .WithUserId(_userId)
            .WithStorageUsed(1048576) // 1MB
            .WithFilesUploadedToday(5)
            .WithMangasCreated(3)
            .Build();

        // Assert
        userQuota.UserId.Should().Be(_userId);
        userQuota.StorageUsedBytes.Should().Be(1048576);
        userQuota.FilesUploadedToday.Should().Be(5);
        userQuota.MangasCreated.Should().Be(3);
    }
}