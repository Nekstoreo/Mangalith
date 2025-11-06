using FluentAssertions;
using Microsoft.Extensions.Logging;
using Mangalith.Application.Common.Exceptions;
using Mangalith.Application.Interfaces.Repositories;
using Mangalith.Application.Interfaces.Services;
using Mangalith.Application.Services;
using Mangalith.Domain.Entities;
using Mangalith.Domain.Enums;
using Mangalith.Tests.TestHelpers;
using Moq;

namespace Mangalith.Tests.Application.Services;

public class PublicationServiceTests
{
    private readonly Mock<IPublicationRepository> _publicationRepositoryMock;
    private readonly Mock<IMangaRepository> _mangaRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IChapterRepository> _chapterRepositoryMock;
    private readonly Mock<IPublicationValidationService> _validationServiceMock;
    private readonly Mock<ILogger<PublicationService>> _loggerMock;
    private readonly PublicationService _publicationService;
    
    private readonly Guid _mangaId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _moderatorId = Guid.NewGuid();
    private readonly Guid _publicationId = Guid.NewGuid();

    public PublicationServiceTests()
    {
        _publicationRepositoryMock = new Mock<IPublicationRepository>();
        _mangaRepositoryMock = new Mock<IMangaRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _chapterRepositoryMock = new Mock<IChapterRepository>();
        _validationServiceMock = new Mock<IPublicationValidationService>();
        _loggerMock = new Mock<ILogger<PublicationService>>();
        
        _publicationService = new PublicationService(
            _publicationRepositoryMock.Object,
            _mangaRepositoryMock.Object,
            _userRepositoryMock.Object,
            _chapterRepositoryMock.Object,
            _validationServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task CreatePublicationAsync_WithValidParameters_ShouldCreateAndReturnPublication()
    {
        // Arrange
        var expectedPublication = new Publication(_mangaId, _userId);

        _validationServiceMock
            .Setup(x => x.ValidatePublicationCreationAsync(_mangaId, _userId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _publicationRepositoryMock
            .Setup(x => x.GetByMangaIdAsync(_mangaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Publication?)null);

        _publicationRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Publication>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPublication);

        // Act
        var result = await _publicationService.CreatePublicationAsync(_mangaId, _userId);

        // Assert
        result.Should().NotBeNull();
        result.MangaId.Should().Be(_mangaId);
        result.CreatedByUserId.Should().Be(_userId);
        result.Status.Should().Be(PublicationStatus.Draft);

        _validationServiceMock.Verify(x => x.ValidatePublicationCreationAsync(_mangaId, _userId, It.IsAny<CancellationToken>()), Times.Once);
        _publicationRepositoryMock.Verify(x => x.GetByMangaIdAsync(_mangaId, It.IsAny<CancellationToken>()), Times.Once);
        _publicationRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Publication>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreatePublicationAsync_WhenPublicationAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        var existingPublication = new Publication(_mangaId, _userId);

        _validationServiceMock
            .Setup(x => x.ValidatePublicationCreationAsync(_mangaId, _userId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _publicationRepositoryMock
            .Setup(x => x.GetByMangaIdAsync(_mangaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPublication);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ConflictAppException>(
            () => _publicationService.CreatePublicationAsync(_mangaId, _userId));

        exception.Message.Should().Be("A publication already exists for this manga");
        _publicationRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Publication>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SubmitForReviewAsync_WithValidPublication_ShouldSubmitAndReturnUpdatedPublication()
    {
        // Arrange
        var publication = TestDataBuilder.CreatePublication()
            .WithMangaId(_mangaId)
            .WithCreatedByUserId(_userId)
            .WithStatus(PublicationStatus.Draft)
            .Build();

        _publicationRepositoryMock
            .Setup(x => x.GetByIdAsync(_publicationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(publication);

        _validationServiceMock
            .Setup(x => x.ValidatePublicationSubmissionAsync(publication, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _publicationRepositoryMock
            .Setup(x => x.UpdateAsync(publication, It.IsAny<CancellationToken>()))
            .ReturnsAsync(publication);

        // Act
        var result = await _publicationService.SubmitForReviewAsync(_publicationId, _userId);

        // Assert
        result.Should().Be(publication);
        result.Status.Should().Be(PublicationStatus.InReview);
        result.SubmittedAtUtc.Should().NotBeNull();

        _validationServiceMock.Verify(x => x.ValidatePublicationSubmissionAsync(publication, It.IsAny<CancellationToken>()), Times.Once);
        _publicationRepositoryMock.Verify(x => x.UpdateAsync(publication, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SubmitForReviewAsync_WithNonExistentPublication_ShouldThrowNotFoundException()
    {
        // Arrange
        _publicationRepositoryMock
            .Setup(x => x.GetByIdAsync(_publicationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Publication?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _publicationService.SubmitForReviewAsync(_publicationId, _userId));

        exception.Message.Should().Contain(_publicationId.ToString());
        exception.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task SubmitForReviewAsync_WithDifferentUser_ShouldThrowForbiddenException()
    {
        // Arrange
        var differentUserId = Guid.NewGuid();
        var publication = TestDataBuilder.CreatePublication()
            .WithCreatedByUserId(_userId)
            .Build();

        _publicationRepositoryMock
            .Setup(x => x.GetByIdAsync(_publicationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(publication);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ForbiddenAppException>(
            () => _publicationService.SubmitForReviewAsync(_publicationId, differentUserId));

        exception.Message.Should().Be("You can only submit your own publications");
    }

    [Fact]
    public async Task ApprovePublicationAsync_WithValidParameters_ShouldApproveAndReturnPublication()
    {
        // Arrange
        var publication = TestDataBuilder.CreatePublication()
            .WithStatus(PublicationStatus.InReview)
            .Build();
        var rating = ContentRating.Teen;
        var isNsfw = true;
        var comments = "Approved with minor concerns";

        _publicationRepositoryMock
            .Setup(x => x.GetByIdAsync(_publicationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(publication);

        _validationServiceMock
            .Setup(x => x.ValidateModeratorPermissionsAsync(_moderatorId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _validationServiceMock
            .Setup(x => x.ValidateModerationAction(It.IsAny<PublicationStatus>(), It.IsAny<PublicationStatus>(), It.IsAny<string>(), It.IsAny<string>()))
            .Verifiable();

        _publicationRepositoryMock
            .Setup(x => x.UpdateAsync(publication, It.IsAny<CancellationToken>()))
            .ReturnsAsync(publication);

        // Act
        var result = await _publicationService.ApprovePublicationAsync(_publicationId, _moderatorId, rating, isNsfw, comments);

        // Assert
        result.Should().Be(publication);
        result.Status.Should().Be(PublicationStatus.Published);
        result.ContentRating.Should().Be(rating);
        result.IsNsfw.Should().Be(isNsfw);
        result.ModeratorComments.Should().Be(comments);
        result.ReviewedByUserId.Should().Be(_moderatorId);

        _validationServiceMock.Verify(x => x.ValidateModeratorPermissionsAsync(_moderatorId, It.IsAny<CancellationToken>()), Times.Once);
        _validationServiceMock.Verify(x => x.ValidateModerationAction(It.IsAny<PublicationStatus>(), It.IsAny<PublicationStatus>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Theory]
    [InlineData((ContentRating)999)]
    [InlineData((ContentRating)(-1))]
    public async Task ApprovePublicationAsync_WithInvalidContentRating_ShouldThrowModerationValidationException(ContentRating invalidRating)
    {
        // Arrange
        var publication = TestDataBuilder.CreatePublication()
            .WithStatus(PublicationStatus.InReview)
            .Build();

        _publicationRepositoryMock
            .Setup(x => x.GetByIdAsync(_publicationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(publication);

        _validationServiceMock
            .Setup(x => x.ValidateModeratorPermissionsAsync(_moderatorId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ModerationValidationException>(
            () => _publicationService.ApprovePublicationAsync(_publicationId, _moderatorId, invalidRating, false));

        exception.Message.Should().Contain("Invalid content rating specified");
    }

    [Fact]
    public async Task RejectPublicationAsync_WithValidParameters_ShouldRejectAndReturnPublication()
    {
        // Arrange
        var publication = TestDataBuilder.CreatePublication()
            .WithStatus(PublicationStatus.InReview)
            .Build();
        var reason = "Inappropriate content";
        var comments = "Contains explicit material not suitable for platform";

        _publicationRepositoryMock
            .Setup(x => x.GetByIdAsync(_publicationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(publication);

        _validationServiceMock
            .Setup(x => x.ValidateModeratorPermissionsAsync(_moderatorId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _validationServiceMock
            .Setup(x => x.ValidateModerationAction(It.IsAny<PublicationStatus>(), It.IsAny<PublicationStatus>(), It.IsAny<string>(), It.IsAny<string>()))
            .Verifiable();

        _publicationRepositoryMock
            .Setup(x => x.UpdateAsync(publication, It.IsAny<CancellationToken>()))
            .ReturnsAsync(publication);

        // Act
        var result = await _publicationService.RejectPublicationAsync(_publicationId, _moderatorId, reason, comments);

        // Assert
        result.Should().Be(publication);
        result.Status.Should().Be(PublicationStatus.Rejected);
        result.RejectionReason.Should().Be(reason);
        result.ModeratorComments.Should().Be(comments);
        result.ReviewedByUserId.Should().Be(_moderatorId);

        _validationServiceMock.Verify(x => x.ValidateModeratorPermissionsAsync(_moderatorId, It.IsAny<CancellationToken>()), Times.Once);
        _validationServiceMock.Verify(x => x.ValidateModerationAction(PublicationStatus.InReview, PublicationStatus.Rejected, comments, reason), Times.Once);
    }

    [Fact]
    public async Task RequestRevisionAsync_WithValidParameters_ShouldRequestRevisionAndReturnPublication()
    {
        // Arrange
        var publication = TestDataBuilder.CreatePublication()
            .WithStatus(PublicationStatus.InReview)
            .Build();
        var comments = "Please add more detailed description and fix formatting";

        _publicationRepositoryMock
            .Setup(x => x.GetByIdAsync(_publicationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(publication);

        _validationServiceMock
            .Setup(x => x.ValidateModeratorPermissionsAsync(_moderatorId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _validationServiceMock
            .Setup(x => x.ValidateModerationAction(It.IsAny<PublicationStatus>(), It.IsAny<PublicationStatus>(), It.IsAny<string>(), It.IsAny<string>()))
            .Verifiable();

        _publicationRepositoryMock
            .Setup(x => x.UpdateAsync(publication, It.IsAny<CancellationToken>()))
            .ReturnsAsync(publication);

        // Act
        var result = await _publicationService.RequestRevisionAsync(_publicationId, _moderatorId, comments);

        // Assert
        result.Should().Be(publication);
        result.Status.Should().Be(PublicationStatus.NeedsRevision);
        result.ModeratorComments.Should().Be(comments);
        result.ReviewedByUserId.Should().Be(_moderatorId);

        _validationServiceMock.Verify(x => x.ValidateModeratorPermissionsAsync(_moderatorId, It.IsAny<CancellationToken>()), Times.Once);
        _validationServiceMock.Verify(x => x.ValidateModerationAction(It.IsAny<PublicationStatus>(), It.IsAny<PublicationStatus>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ArchivePublicationAsync_ByCreator_ShouldArchiveSuccessfully()
    {
        // Arrange
        var publication = TestDataBuilder.CreatePublication()
            .WithCreatedByUserId(_userId)
            .WithStatus(PublicationStatus.Published)
            .Build();
        var user = TestDataBuilder.CreateUser()
            .WithRole(UserRole.Reader)
            .Build();

        _publicationRepositoryMock
            .Setup(x => x.GetByIdAsync(_publicationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(publication);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _publicationRepositoryMock
            .Setup(x => x.UpdateAsync(publication, It.IsAny<CancellationToken>()))
            .ReturnsAsync(publication);

        // Act
        var result = await _publicationService.ArchivePublicationAsync(_publicationId, _userId);

        // Assert
        result.Should().Be(publication);
        result.Status.Should().Be(PublicationStatus.Archived);
    }

    [Fact]
    public async Task ArchivePublicationAsync_ByAdmin_ShouldArchiveSuccessfully()
    {
        // Arrange
        var differentUserId = Guid.NewGuid();
        var publication = TestDataBuilder.CreatePublication()
            .WithCreatedByUserId(_userId)
            .WithStatus(PublicationStatus.Published)
            .Build();
        var adminUser = TestDataBuilder.CreateUser()
            .WithRole(UserRole.Administrator)
            .Build();

        _publicationRepositoryMock
            .Setup(x => x.GetByIdAsync(_publicationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(publication);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(differentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(adminUser);

        _publicationRepositoryMock
            .Setup(x => x.UpdateAsync(publication, It.IsAny<CancellationToken>()))
            .ReturnsAsync(publication);

        // Act
        var result = await _publicationService.ArchivePublicationAsync(_publicationId, differentUserId);

        // Assert
        result.Should().Be(publication);
        result.Status.Should().Be(PublicationStatus.Archived);
    }

    [Fact]
    public async Task ArchivePublicationAsync_ByUnauthorizedUser_ShouldThrowForbiddenException()
    {
        // Arrange
        var unauthorizedUserId = Guid.NewGuid();
        var publication = TestDataBuilder.CreatePublication()
            .WithCreatedByUserId(_userId)
            .WithStatus(PublicationStatus.Published)
            .Build();
        var regularUser = TestDataBuilder.CreateUser()
            .WithRole(UserRole.Reader)
            .Build();

        _publicationRepositoryMock
            .Setup(x => x.GetByIdAsync(_publicationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(publication);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(unauthorizedUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(regularUser);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ForbiddenAppException>(
            () => _publicationService.ArchivePublicationAsync(_publicationId, unauthorizedUserId));

        exception.Message.Should().Be("Only the creator or an administrator can archive this publication");
    }

    [Fact]
    public async Task GetPublicationByIdAsync_WithValidId_ShouldReturnPublication()
    {
        // Arrange
        var publication = TestDataBuilder.CreatePublication().Build();

        _publicationRepositoryMock
            .Setup(x => x.GetByIdAsync(_publicationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(publication);

        // Act
        var result = await _publicationService.GetPublicationByIdAsync(_publicationId);

        // Assert
        result.Should().Be(publication);
        _publicationRepositoryMock.Verify(x => x.GetByIdAsync(_publicationId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPublicationByIdAsync_WithNonExistentId_ShouldReturnNull()
    {
        // Arrange
        _publicationRepositoryMock
            .Setup(x => x.GetByIdAsync(_publicationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Publication?)null);

        // Act
        var result = await _publicationService.GetPublicationByIdAsync(_publicationId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPublicationByMangaIdAsync_WithValidMangaId_ShouldReturnPublication()
    {
        // Arrange
        var publication = TestDataBuilder.CreatePublication()
            .WithMangaId(_mangaId)
            .Build();

        _publicationRepositoryMock
            .Setup(x => x.GetByMangaIdAsync(_mangaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(publication);

        // Act
        var result = await _publicationService.GetPublicationByMangaIdAsync(_mangaId);

        // Assert
        result.Should().Be(publication);
        result!.MangaId.Should().Be(_mangaId);
    }
}