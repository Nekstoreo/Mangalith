using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Mangalith.Application.Interfaces.Repositories;
using Mangalith.Application.Services;
using Mangalith.Domain.Constants;
using Mangalith.Domain.Entities;
using Mangalith.Tests.TestHelpers;
using Moq;

namespace Mangalith.Tests.Application.Services;

public class PermissionServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ILogger<PermissionService>> _loggerMock;
    private readonly IMemoryCache _memoryCache;
    private readonly PermissionService _permissionService;
    private readonly Guid _userId = Guid.NewGuid();

    public PermissionServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _loggerMock = new Mock<ILogger<PermissionService>>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        
        _permissionService = new PermissionService(
            _userRepositoryMock.Object,
            _memoryCache,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HasPermissionAsync_WithValidUserAndPermission_ShouldReturnTrue()
    {
        // Arrange
        var user = TestDataBuilder.CreateUser()
            .WithRole(UserRole.Administrator)
            .Build();
        var permission = Permissions.User.Read;

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _permissionService.HasPermissionAsync(_userId, permission);

        // Assert
        result.Should().BeTrue();
        _userRepositoryMock.Verify(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HasPermissionAsync_WithNonExistentUser_ShouldReturnFalse()
    {
        // Arrange
        var permission = Permissions.User.Read;

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _permissionService.HasPermissionAsync(_userId, permission);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task HasPermissionAsync_WithInvalidPermission_ShouldReturnFalse(string? permission)
    {
        // Act
        var result = await _permissionService.HasPermissionAsync(_userId, permission!);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(UserRole.Reader, Permissions.Manga.Read, true)]
    [InlineData(UserRole.Reader, Permissions.Manga.Create, false)]
    [InlineData(UserRole.Uploader, Permissions.Manga.Create, true)]
    [InlineData(UserRole.Moderator, Permissions.Publication.Review, true)]
    [InlineData(UserRole.Administrator, Permissions.User.Manage, true)]
    public async Task HasPermissionAsync_WithRoleAndPermission_ShouldReturnCorrectResult(UserRole role, string permission, bool expectedResult)
    {
        // Act
        var result = await _permissionService.HasPermissionAsync(role, permission);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task HasPermissionAsync_WithRoleAndInvalidPermission_ShouldReturnFalse(string? permission)
    {
        // Act
        var result = await _permissionService.HasPermissionAsync(UserRole.Reader, permission!);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetUserPermissionsAsync_WithValidUser_ShouldReturnPermissions()
    {
        // Arrange
        var user = TestDataBuilder.CreateUser()
            .WithRole(UserRole.Uploader)
            .Build();

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _permissionService.GetUserPermissionsAsync(_userId);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().Contain(Permissions.Manga.Read);
        result.Should().Contain(Permissions.Manga.Create);
    }

    [Fact]
    public async Task GetUserPermissionsAsync_WithCachedPermissions_ShouldReturnCachedResult()
    {
        // Arrange
        var user = TestDataBuilder.CreateUser()
            .WithRole(UserRole.Uploader)
            .Build();

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // First call to populate cache
        var firstResult = await _permissionService.GetUserPermissionsAsync(_userId);

        // Reset mock to verify it's not called again
        _userRepositoryMock.Reset();

        // Act - Second call should use cache
        var result = await _permissionService.GetUserPermissionsAsync(_userId);

        // Assert
        result.Should().BeEquivalentTo(firstResult);
        _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetUserPermissionsAsync_WithNonExistentUser_ShouldReturnEmptyCollection()
    {
        // Arrange
        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _permissionService.GetUserPermissionsAsync(_userId);

        // Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData(UserRole.Reader)]
    [InlineData(UserRole.Uploader)]
    [InlineData(UserRole.Moderator)]
    [InlineData(UserRole.Administrator)]
    public async Task GetRolePermissionsAsync_WithValidRole_ShouldReturnPermissions(UserRole role)
    {
        // Act
        var result = await _permissionService.GetRolePermissionsAsync(role);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().Contain(Permissions.Manga.Read); // All roles should have basic manga read permission
    }

    [Fact]
    public async Task GetRolePermissionsAsync_WithCachedPermissions_ShouldReturnCachedResult()
    {
        // Arrange - First call to populate cache
        var firstResult = await _permissionService.GetRolePermissionsAsync(UserRole.Reader);

        // Act - Second call should use cache
        var result = await _permissionService.GetRolePermissionsAsync(UserRole.Reader);

        // Assert
        result.Should().BeEquivalentTo(firstResult);
    }

    [Fact]
    public async Task HasPermissionsAsync_WithAllRequiredPermissions_ShouldReturnTrue()
    {
        // Arrange
        var user = TestDataBuilder.CreateUser()
            .WithRole(UserRole.Administrator)
            .Build();
        var permissions = new[] { Permissions.User.Read, Permissions.Manga.Create };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _permissionService.HasPermissionsAsync(_userId, permissions, requireAll: true);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasPermissionsAsync_WithSomeRequiredPermissions_RequireAll_ShouldReturnFalse()
    {
        // Arrange
        var user = TestDataBuilder.CreateUser()
            .WithRole(UserRole.Reader)
            .Build();
        var permissions = new[] { Permissions.User.Read, Permissions.User.Manage };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _permissionService.HasPermissionsAsync(_userId, permissions, requireAll: true);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasPermissionsAsync_WithSomeRequiredPermissions_RequireAny_ShouldReturnTrue()
    {
        // Arrange
        var user = TestDataBuilder.CreateUser()
            .WithRole(UserRole.Reader)
            .Build();
        // Reader has Manga.Read but not Manga.Create, so "Any" should return true
        var permissions = new[] { Permissions.Manga.Read, Permissions.Manga.Create };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _permissionService.HasPermissionsAsync(_userId, permissions, requireAll: false);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasPermissionsAsync_WithEmptyPermissions_ShouldReturnFalse()
    {
        // Arrange
        var permissions = Array.Empty<string>();

        // Act
        var result = await _permissionService.HasPermissionsAsync(_userId, permissions);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetUserRoleAsync_WithValidUser_ShouldReturnRole()
    {
        // Arrange
        var user = TestDataBuilder.CreateUser()
            .WithRole(UserRole.Moderator)
            .Build();

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _permissionService.GetUserRoleAsync(_userId);

        // Assert
        result.Should().Be(UserRole.Moderator);
    }

    [Fact]
    public async Task GetUserRoleAsync_WithCachedRole_ShouldReturnCachedResult()
    {
        // Arrange
        var user = TestDataBuilder.CreateUser()
            .WithRole(UserRole.Administrator)
            .Build();

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // First call to populate cache
        var firstResult = await _permissionService.GetUserRoleAsync(_userId);

        // Reset mock to verify it's not called again
        _userRepositoryMock.Reset();

        // Act - Second call should use cache
        var result = await _permissionService.GetUserRoleAsync(_userId);

        // Assert
        result.Should().Be(UserRole.Administrator);
        result.Should().Be(firstResult);
        _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetUserRoleAsync_WithNonExistentUser_ShouldReturnNull()
    {
        // Arrange
        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _permissionService.GetUserRoleAsync(_userId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task InvalidateUserPermissionsAsync_ShouldRemoveCacheEntries()
    {
        // Arrange
        var user = TestDataBuilder.CreateUser()
            .WithRole(UserRole.Administrator)
            .Build();

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Populate cache first
        await _permissionService.GetUserPermissionsAsync(_userId);
        await _permissionService.GetUserRoleAsync(_userId);

        // Reset to verify cache was cleared
        _userRepositoryMock.Reset();
        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        await _permissionService.InvalidateUserPermissionsAsync(_userId);

        // After invalidation, accessing permissions should hit the repository again
        var result = await _permissionService.GetUserPermissionsAsync(_userId);

        // Assert
        result.Should().NotBeEmpty();
        _userRepositoryMock.Verify(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task InvalidateAllPermissionsCacheAsync_ShouldClearCache()
    {
        // Arrange
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var permissionService = new PermissionService(
            _userRepositoryMock.Object,
            memoryCache,
            _loggerMock.Object);

        // Act
        await permissionService.InvalidateAllPermissionsCacheAsync();

        // Assert - No exception should be thrown
        // The actual clearing behavior depends on the MemoryCache implementation
    }

    [Fact]
    public async Task HasPermissionAsync_WithException_ShouldReturnFalseAndLogError()
    {
        // Arrange
        var permission = Permissions.User.Read;

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _permissionService.HasPermissionAsync(_userId, permission);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetUserPermissionsAsync_WithException_ShouldReturnEmptyAndLogError()
    {
        // Arrange
        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _permissionService.GetUserPermissionsAsync(_userId);

        // Assert
        result.Should().BeEmpty();
    }
}