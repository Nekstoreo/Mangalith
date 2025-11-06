using FluentAssertions;
using Microsoft.Extensions.Logging;
using Mangalith.Application.Common.Exceptions;
using Mangalith.Application.Common.Models;
using Mangalith.Application.Interfaces.Repositories;
using Mangalith.Application.Interfaces.Services;
using Mangalith.Application.Services;
using Mangalith.Domain.Entities;
using Mangalith.Domain.Enums;
using Moq;

namespace Mangalith.Tests.Application.Services;

public class ModerationServiceTests
{
    private readonly Mock<IPublicationRepository> _publicationRepo = new();
    private readonly Mock<IModerationActionRepository> _moderationRepo = new();
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IPublicationService> _publicationService = new();
    private readonly Mock<IPublicationValidationService> _validation = new();
    private readonly Mock<ILogger<ModerationService>> _logger = new();
    private readonly ModerationService _service;

    private readonly Guid _publicationId = Guid.NewGuid();
    private readonly Guid _moderatorId = Guid.NewGuid();

    public ModerationServiceTests()
    {
        _service = new ModerationService(
            _publicationRepo.Object,
            _moderationRepo.Object,
            _userRepo.Object,
            _publicationService.Object,
            _validation.Object,
            _logger.Object
        );
    }

    [Fact]
    public async Task CreateModerationActionAsync_WithValidData_ShouldCreateAction()
    {
        // Arrange
        var publication = new Publication(Guid.NewGuid(), Guid.NewGuid());
        var moderator = new User("mod@example.com", "hash", "Mod", null);
        moderator.UpdateRole(UserRole.Moderator);

        _publicationRepo.Setup(r => r.GetByIdAsync(_publicationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(publication);
        _userRepo.Setup(r => r.GetByIdAsync(_moderatorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(moderator);
        _moderationRepo.Setup(r => r.CreateAsync(It.IsAny<ModerationAction>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ModerationAction m, CancellationToken _) => m);

        // Act
        var created = await _service.CreateModerationActionAsync(
            _publicationId,
            _moderatorId,
            ModerationActionType.RequestedRevision,
            "Please fix metadata",
            CancellationToken.None);

        // Assert
        created.Should().NotBeNull();
        created.ActionType.Should().Be(ModerationActionType.RequestedRevision);
        created.PublicationId.Should().Be(_publicationId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateModerationActionAsync_MissingComments_ShouldThrow(string comments)
    {
        // Arrange
        var publication = new Publication(Guid.NewGuid(), Guid.NewGuid());
        var moderator = new User("mod@example.com", "hash", "Mod", null);
        moderator.UpdateRole(UserRole.Moderator);
        _publicationRepo.Setup(r => r.GetByIdAsync(_publicationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(publication);
        _userRepo.Setup(r => r.GetByIdAsync(_moderatorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(moderator);

        // Act
        Func<Task> act = () => _service.CreateModerationActionAsync(
            _publicationId, _moderatorId, ModerationActionType.Rejected, comments, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ModerationValidationException>()
            .WithMessage("*Comments are required for moderation actions*");
    }

    [Fact]
    public async Task BulkModerationActionAsync_WithNoPublications_ShouldThrow()
    {
        var moderator = new User("mod@example.com", "hash", "Mod", null);
        moderator.UpdateRole(UserRole.Moderator);
        _userRepo.Setup(r => r.GetByIdAsync(_moderatorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(moderator);

        Func<Task> act = () => _service.BulkModerationActionAsync(
            Array.Empty<Guid>(), ModerationActionType.Approved, _moderatorId, "ok", CancellationToken.None);

        await act.Should().ThrowAsync<ModerationException>()
            .WithMessage("*No publications were successfully processed*");
    }

    [Fact]
    public async Task GetModerationStatisticsAsync_WhenRepoThrows_ShouldWrapAsModerationException()
    {
        _publicationRepo.Setup(r => r.GetCountByStatusAsync(It.IsAny<PublicationStatus>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("db error"));

        Func<Task> act = () => _service.GetModerationStatisticsAsync();

        await act.Should().ThrowAsync<ModerationException>()
            .WithMessage("*Failed to generate moderation statistics*");
    }
}
