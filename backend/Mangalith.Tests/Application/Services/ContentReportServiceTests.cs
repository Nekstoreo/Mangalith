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

public class ContentReportServiceTests
{
    private readonly Mock<IContentReportRepository> _reportRepo = new();
    private readonly Mock<IPublicationRepository> _publicationRepo = new();
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IPublicationValidationService> _validation = new();
    private readonly Mock<IMangaRepository> _mangaRepo = new();
    private readonly Mock<ILogger<ContentReportService>> _logger = new();
    private readonly ContentReportService _service;

    private readonly Guid _publicationId = Guid.NewGuid();
    private readonly Guid _reporterId = Guid.NewGuid();

    public ContentReportServiceTests()
    {
        _service = new ContentReportService(
            _reportRepo.Object,
            _publicationRepo.Object,
            _userRepo.Object,
            _validation.Object,
            _logger.Object
        );
    }

    [Fact]
    public async Task CreateReportAsync_SelfReport_ShouldThrowValidation()
    {
        // Arrange: publicación creada por el mismo usuario
        var publication = new Publication(Guid.NewGuid(), _reporterId);
        var reporter = new User("reporter@example.com", "hash", "Reporter", null);
        
        _validation.Setup(v => v.ValidateContentReportAsync(_publicationId, _reporterId, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _publicationRepo.Setup(r => r.GetByIdAsync(_publicationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(publication);
        _reportRepo.Setup(r => r.GetByPublicationAndUserAsync(_publicationId, _reporterId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ContentReport>());

        // Act
        Func<Task> act = () => _service.CreateReportAsync(
            _publicationId, _reporterId, ContentReportCategory.Spam, "Descripción válida", CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ContentReportValidationException>()
            .WithMessage("*You cannot report your own content*");
    }

    [Fact]
    public async Task CreateReportAsync_DuplicatePending_ShouldThrowValidation()
    {
        var publication = new Publication(Guid.NewGuid(), Guid.NewGuid());
        var reporter = new User("reporter@example.com", "hash", "Reporter", null);
        
        _validation.Setup(v => v.ValidateContentReportAsync(_publicationId, _reporterId, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _publicationRepo.Setup(r => r.GetByIdAsync(_publicationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(publication);
        _reportRepo.Setup(r => r.GetByPublicationAndUserAsync(_publicationId, _reporterId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ContentReport>
            {
                new ContentReport(_publicationId, _reporterId, ContentReportCategory.Spam, "dup")
            });

        Func<Task> act = () => _service.CreateReportAsync(
            _publicationId, _reporterId, ContentReportCategory.Spam, "Descripción válida", CancellationToken.None);

        await act.Should().ThrowAsync<ContentReportValidationException>()
            .WithMessage("*already reported this content*");
    }

    [Fact]
    public async Task ReviewReportAsync_InvalidStatus_ShouldThrowValidation()
    {
        var moderator = new User("mod@example.com", "hash", "Mod", null);
        moderator.UpdateRole(UserRole.Moderator);
        _userRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(moderator);

        var report = new ContentReport(_publicationId, Guid.NewGuid(), ContentReportCategory.Spam, "desc");
        // Set to reviewed to make invalid for review
        report.MarkUnderReview(Guid.NewGuid());
        report.Resolve(Guid.NewGuid(), "done");
        _reportRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);

    Func<Task> act = () => _service.ReviewReportAsync(Guid.NewGuid(), Guid.NewGuid(), ContentReportStatus.UnderReview, null, CancellationToken.None);

        await act.Should().ThrowAsync<ContentReportValidationException>();
    }
}
