using FluentAssertions;
using Microsoft.Extensions.Logging;
using Mangalith.Application.Common.Exceptions;
using Mangalith.Application.Interfaces.Repositories;
using Mangalith.Application.Services;
using Mangalith.Domain.Constants;
using Mangalith.Domain.Entities;
using Mangalith.Tests.TestHelpers;
using Moq;

namespace Mangalith.Tests.Application.Services;

public class QuotaServiceTests
{
    private readonly Mock<IUserQuotaRepository> _userQuotaRepositoryMock;
    private readonly Mock<IRateLimitRepository> _rateLimitRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IMangaFileRepository> _mangaFileRepositoryMock;
    private readonly Mock<ILogger<QuotaService>> _loggerMock;
    private readonly QuotaService _quotaService;
    private readonly Guid _userId = Guid.NewGuid();

    public QuotaServiceTests()
    {
        _userQuotaRepositoryMock = new Mock<IUserQuotaRepository>();
        _rateLimitRepositoryMock = new Mock<IRateLimitRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _mangaFileRepositoryMock = new Mock<IMangaFileRepository>();
        _loggerMock = new Mock<ILogger<QuotaService>>();
        
        _quotaService = new QuotaService(
            _userQuotaRepositoryMock.Object,
            _rateLimitRepositoryMock.Object,
            _userRepositoryMock.Object,
            _mangaFileRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task CanUploadFileAsync_WithValidUserAndFile_ShouldReturnTrue()
    {
        // Arrange
        var user = TestDataBuilder.CreateUser()
            .WithRole(UserRole.Uploader)
            .Build();
        var userQuota = TestDataBuilder.CreateUserQuota()
            .WithUserId(_userId)
            .WithStorageUsed(1048576) // 1MB used
            .WithFilesUploadedToday(5) // 5 files uploaded
            .Build();
        var fileSize = 1048576L; // 1MB file

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userQuotaRepositoryMock
            .Setup(x => x.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userQuota);

        // Act
        var result = await _quotaService.CanUploadFileAsync(_userId, fileSize);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanUploadFileAsync_WithNonExistentUser_ShouldReturnFalse()
    {
        // Arrange
        var fileSize = 1048576L;

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _quotaService.CanUploadFileAsync(_userId, fileSize);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanUploadFileAsync_WithReaderRole_ShouldReturnFalse()
    {
        // Arrange
        var user = TestDataBuilder.CreateUser()
            .WithRole(UserRole.Reader)
            .Build();
        var fileSize = 1048576L;

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _quotaService.CanUploadFileAsync(_userId, fileSize);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanUploadFileAsync_WithFileTooLarge_ShouldReturnFalse()
    {
        // Arrange
        var user = TestDataBuilder.CreateUser()
            .WithRole(UserRole.Uploader)
            .Build();
        var fileSize = QuotaLimits.GetMaxFileSize(UserRole.Uploader) + 1;

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _quotaService.CanUploadFileAsync(_userId, fileSize);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanUploadFileAsync_WithStorageQuotaExceeded_ShouldReturnFalse()
    {
        // Arrange
        var user = TestDataBuilder.CreateUser()
            .WithRole(UserRole.Uploader)
            .Build();
        var storageQuota = QuotaLimits.GetStorageQuota(UserRole.Uploader);
        var userQuota = TestDataBuilder.CreateUserQuota()
            .WithUserId(_userId)
            .WithStorageUsed(storageQuota - 1024) // Almost at limit
            .Build();
        var fileSize = 2048L; // Would exceed quota

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userQuotaRepositoryMock
            .Setup(x => x.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userQuota);

        // Act
        var result = await _quotaService.CanUploadFileAsync(_userId, fileSize);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanUploadFileAsync_WithDailyLimitExceeded_ShouldReturnFalse()
    {
        // Arrange
        var user = TestDataBuilder.CreateUser()
            .WithRole(UserRole.Uploader)
            .Build();
        var dailyLimit = QuotaLimits.GetFileUploadLimit(UserRole.Uploader);
        var userQuota = TestDataBuilder.CreateUserQuota()
            .WithUserId(_userId)
            .WithFilesUploadedToday(dailyLimit)
            .Build();
        var fileSize = 1048576L;

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userQuotaRepositoryMock
            .Setup(x => x.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userQuota);

        // Act
        var result = await _quotaService.CanUploadFileAsync(_userId, fileSize);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CheckStorageQuotaAsync_WithinQuota_ShouldReturnTrue()
    {
        // Arrange
        var user = TestDataBuilder.CreateUser()
            .WithRole(UserRole.Uploader)
            .Build();
        var userQuota = TestDataBuilder.CreateUserQuota()
            .WithUserId(_userId)
            .WithStorageUsed(1048576) // 1MB used
            .Build();
        var additionalBytes = 1048576L; // 1MB additional

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userQuotaRepositoryMock
            .Setup(x => x.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userQuota);

        // Act
        var result = await _quotaService.CheckStorageQuotaAsync(_userId, additionalBytes);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task TrackFileUploadAsync_ShouldUpdateQuotaAndIncrementCounters()
    {
        // Arrange
        var userQuota = TestDataBuilder.CreateUserQuota()
            .WithUserId(_userId)
            .Build();
        var fileSize = 1048576L;

        _userQuotaRepositoryMock
            .Setup(x => x.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userQuota);

        // Act
        await _quotaService.TrackFileUploadAsync(_userId, fileSize);

        // Assert
        userQuota.StorageUsedBytes.Should().Be(fileSize);
        userQuota.FilesUploadedToday.Should().Be(1);
        _userQuotaRepositoryMock.Verify(x => x.UpdateAsync(userQuota, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TrackFileDeleteAsync_ShouldUpdateQuotaAndDecrementStorage()
    {
        // Arrange
        var userQuota = TestDataBuilder.CreateUserQuota()
            .WithUserId(_userId)
            .WithStorageUsed(2097152) // 2MB used
            .Build();
        var fileSize = 1048576L; // 1MB to delete

        _userQuotaRepositoryMock
            .Setup(x => x.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userQuota);

        // Act
        await _quotaService.TrackFileDeleteAsync(_userId, fileSize);

        // Assert
        userQuota.StorageUsedBytes.Should().Be(1048576); // 1MB remaining
        _userQuotaRepositoryMock.Verify(x => x.UpdateAsync(userQuota, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUserQuotaAsync_WithExistingQuota_ShouldReturnQuota()
    {
        // Arrange
        var existingQuota = TestDataBuilder.CreateUserQuota()
            .WithUserId(_userId)
            .Build();

        _userQuotaRepositoryMock
            .Setup(x => x.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingQuota);

        // Act
        var result = await _quotaService.GetUserQuotaAsync(_userId);

        // Assert
        result.Should().Be(existingQuota);
        _userQuotaRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<UserQuota>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetUserQuotaAsync_WithoutExistingQuota_ShouldCreateNewQuota()
    {
        // Arrange
        _userQuotaRepositoryMock
            .Setup(x => x.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserQuota?)null);

        _userQuotaRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<UserQuota>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserQuota quota, CancellationToken _) => quota);

        // Act
        var result = await _quotaService.GetUserQuotaAsync(_userId);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(_userId);
        _userQuotaRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<UserQuota>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(UserRole.Reader, false)]
    [InlineData(UserRole.Uploader, true)]
    [InlineData(UserRole.Moderator, true)]
    [InlineData(UserRole.Administrator, true)]
    public async Task CanCreateMangaAsync_WithDifferentRoles_ShouldReturnCorrectResult(UserRole role, bool expectedResult)
    {
        // Arrange
        var user = TestDataBuilder.CreateUser()
            .WithRole(role)
            .Build();
        var userQuota = TestDataBuilder.CreateUserQuota()
            .WithUserId(_userId)
            .Build();

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userQuotaRepositoryMock
            .Setup(x => x.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userQuota);

        // Act
        var result = await _quotaService.CanCreateMangaAsync(_userId);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public async Task CanCreateMangaAsync_WithMangaLimitExceeded_ShouldReturnFalse()
    {
        // Arrange
        var user = TestDataBuilder.CreateUser()
            .WithRole(UserRole.Uploader)
            .Build();
        var mangaLimit = QuotaLimits.GetMaxMangaCreations(UserRole.Uploader);
        var userQuota = TestDataBuilder.CreateUserQuota()
            .WithUserId(_userId)
            .WithMangasCreated(mangaLimit)
            .Build();

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userQuotaRepositoryMock
            .Setup(x => x.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userQuota);

        // Act
        var result = await _quotaService.CanCreateMangaAsync(_userId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task TrackMangaCreationAsync_ShouldIncrementMangaCounter()
    {
        // Arrange
        var userQuota = TestDataBuilder.CreateUserQuota()
            .WithUserId(_userId)
            .Build();

        _userQuotaRepositoryMock
            .Setup(x => x.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userQuota);

        // Act
        await _quotaService.TrackMangaCreationAsync(_userId);

        // Assert
        userQuota.MangasCreated.Should().Be(1);
        _userQuotaRepositoryMock.Verify(x => x.UpdateAsync(userQuota, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TrackMangaDeletionAsync_ShouldDecrementMangaCounter()
    {
        // Arrange
        var userQuota = TestDataBuilder.CreateUserQuota()
            .WithUserId(_userId)
            .WithMangasCreated(3)
            .Build();

        _userQuotaRepositoryMock
            .Setup(x => x.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userQuota);

        // Act
        await _quotaService.TrackMangaDeletionAsync(_userId);

        // Assert
        userQuota.MangasCreated.Should().Be(2);
        _userQuotaRepositoryMock.Verify(x => x.UpdateAsync(userQuota, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetQuotaUsageReportAsync_WithValidUser_ShouldReturnReport()
    {
        // Arrange
        var user = TestDataBuilder.CreateUser()
            .WithRole(UserRole.Uploader)
            .Build();
        var userQuota = TestDataBuilder.CreateUserQuota()
            .WithUserId(_userId)
            .WithStorageUsed(1048576) // 1MB
            .WithFilesUploadedToday(5)
            .WithMangasCreated(3)
            .Build();

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userQuotaRepositoryMock
            .Setup(x => x.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userQuota);

        // Act
        var result = await _quotaService.GetQuotaUsageReportAsync(_userId);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(_userId);
        result.UserRole.Should().Be(UserRole.Uploader);
        result.StorageUsedBytes.Should().Be(1048576);
        result.FilesUploadedToday.Should().Be(5);
        result.MangasCreated.Should().Be(3);
        result.StorageQuotaBytes.Should().Be(QuotaLimits.GetStorageQuota(UserRole.Uploader));
        result.DailyUploadLimit.Should().Be(QuotaLimits.GetFileUploadLimit(UserRole.Uploader));
        result.MangaCreationLimit.Should().Be(QuotaLimits.GetMaxMangaCreations(UserRole.Uploader));
    }

    [Fact]
    public async Task GetQuotaUsageReportAsync_WithNonExistentUser_ShouldThrowNotFoundException()
    {
        // Arrange
        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _quotaService.GetQuotaUsageReportAsync(_userId));

        exception.Message.Should().Contain(_userId.ToString());
        exception.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task RecalculateUserStorageUsageAsync_ShouldUpdateStorageBasedOnActualFiles()
    {
        // Arrange
        var userQuota = TestDataBuilder.CreateUserQuota()
            .WithUserId(_userId)
            .WithStorageUsed(5242880) // 5MB recorded
            .Build();

        var userFiles = new List<MangaFile>
        {
            TestDataBuilder.CreateMangaFile().WithFileSize(1048576).Build(), // 1MB
            TestDataBuilder.CreateMangaFile().WithFileSize(2097152).Build(), // 2MB
            TestDataBuilder.CreateMangaFile().WithFileSize(1048576).Build()  // 1MB
        }; // Total: 4MB actual

        _userQuotaRepositoryMock
            .Setup(x => x.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userQuota);

        _mangaFileRepositoryMock
            .Setup(x => x.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userFiles);

        // Act
        await _quotaService.RecalculateUserStorageUsageAsync(_userId);

        // Assert
        userQuota.StorageUsedBytes.Should().Be(4194304); // 4MB actual
        _userQuotaRepositoryMock.Verify(x => x.UpdateAsync(userQuota, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CleanupExpiredRateLimitEntriesAsync_ShouldCallRepositoryCleanup()
    {
        // Act
        await _quotaService.CleanupExpiredRateLimitEntriesAsync();

        // Assert
        _rateLimitRepositoryMock.Verify(x => x.DeleteExpiredEntriesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}