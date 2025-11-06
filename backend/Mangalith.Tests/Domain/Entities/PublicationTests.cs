using FluentAssertions;
using Mangalith.Domain.Entities;
using Mangalith.Domain.Enums;
using Mangalith.Tests.TestHelpers;

namespace Mangalith.Tests.Domain.Entities;

public class PublicationTests
{
    private readonly Guid _mangaId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _moderatorId = Guid.NewGuid();

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreatePublicationWithCorrectProperties()
    {
        // Act
        var publication = new Publication(_mangaId, _userId);

        // Assert
        publication.Id.Should().NotBe(Guid.Empty);
        publication.MangaId.Should().Be(_mangaId);
        publication.CreatedByUserId.Should().Be(_userId);
        publication.Status.Should().Be(PublicationStatus.Draft);
        publication.ContentRating.Should().Be(ContentRating.General);
        publication.IsNsfw.Should().BeFalse();
        publication.ModeratorComments.Should().BeNull();
        publication.RejectionReason.Should().BeNull();
        publication.SubmittedAtUtc.Should().BeNull();
        publication.ReviewedAtUtc.Should().BeNull();
        publication.ReviewedByUserId.Should().BeNull();
        publication.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        publication.UpdatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void SubmitForReview_FromDraftStatus_ShouldUpdateStatusAndTimestamp()
    {
        // Arrange
        var publication = new Publication(_mangaId, _userId);
        var originalUpdatedAt = publication.UpdatedAtUtc;

        // Act
        publication.SubmitForReview();

        // Assert
        publication.Status.Should().Be(PublicationStatus.InReview);
        publication.SubmittedAtUtc.Should().NotBeNull();
        publication.SubmittedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        publication.UpdatedAtUtc.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void SubmitForReview_FromNeedsRevisionStatus_ShouldUpdateStatusAndTimestamp()
    {
        // Arrange
        var publication = TestDataBuilder.CreatePublication()
            .WithStatus(PublicationStatus.NeedsRevision)
            .Build();

        // Act
        publication.SubmitForReview();

        // Assert
        publication.Status.Should().Be(PublicationStatus.InReview);
        publication.SubmittedAtUtc.Should().NotBeNull();
        publication.SubmittedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(PublicationStatus.Published)]
    [InlineData(PublicationStatus.Rejected)]
    [InlineData(PublicationStatus.Archived)]
    public void SubmitForReview_FromInvalidStatus_ShouldThrowInvalidOperationException(PublicationStatus invalidStatus)
    {
        // Arrange
        var publication = TestDataBuilder.CreatePublication()
            .WithStatus(invalidStatus)
            .Build();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => publication.SubmitForReview());
        exception.Message.Should().Contain($"Cannot submit publication with status {invalidStatus}");
    }

    [Fact]
    public void Approve_FromInReviewStatus_ShouldUpdateStatusAndProperties()
    {
        // Arrange
        var publication = TestDataBuilder.CreatePublication()
            .WithStatus(PublicationStatus.InReview)
            .Build();
        var rating = ContentRating.Teen;
        var isNsfw = true;
        var comments = "Approved with minor concerns";

        // Act
        publication.Approve(_moderatorId, rating, isNsfw, comments);

        // Assert
        publication.Status.Should().Be(PublicationStatus.Published);
        publication.ContentRating.Should().Be(rating);
        publication.IsNsfw.Should().Be(isNsfw);
        publication.ModeratorComments.Should().Be(comments);
        publication.ReviewedByUserId.Should().Be(_moderatorId);
        publication.ReviewedAtUtc.Should().NotBeNull();
        publication.ReviewedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(PublicationStatus.Draft)]
    [InlineData(PublicationStatus.Published)]
    [InlineData(PublicationStatus.Rejected)]
    public void Approve_FromInvalidStatus_ShouldThrowInvalidOperationException(PublicationStatus invalidStatus)
    {
        // Arrange
        var publication = TestDataBuilder.CreatePublication()
            .WithStatus(invalidStatus)
            .Build();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => publication.Approve(_moderatorId, ContentRating.General, false));
        exception.Message.Should().Contain($"Cannot approve publication with status {invalidStatus}");
    }

    [Fact]
    public void Reject_FromInReviewStatus_ShouldUpdateStatusAndProperties()
    {
        // Arrange
        var publication = TestDataBuilder.CreatePublication()
            .WithStatus(PublicationStatus.InReview)
            .Build();
        var reason = "Inappropriate content";
        var comments = "Contains explicit material not suitable for platform";

        // Act
        publication.Reject(_moderatorId, reason, comments);

        // Assert
        publication.Status.Should().Be(PublicationStatus.Rejected);
        publication.RejectionReason.Should().Be(reason);
        publication.ModeratorComments.Should().Be(comments);
        publication.ReviewedByUserId.Should().Be(_moderatorId);
        publication.ReviewedAtUtc.Should().NotBeNull();
        publication.ReviewedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(PublicationStatus.Draft)]
    [InlineData(PublicationStatus.Published)]
    [InlineData(PublicationStatus.Rejected)]
    public void Reject_FromInvalidStatus_ShouldThrowInvalidOperationException(PublicationStatus invalidStatus)
    {
        // Arrange
        var publication = TestDataBuilder.CreatePublication()
            .WithStatus(invalidStatus)
            .Build();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => publication.Reject(_moderatorId, "reason", "comments"));
        exception.Message.Should().Contain($"Cannot reject publication with status {invalidStatus}");
    }

    [Fact]
    public void RequestRevision_FromInReviewStatus_ShouldUpdateStatusAndProperties()
    {
        // Arrange
        var publication = TestDataBuilder.CreatePublication()
            .WithStatus(PublicationStatus.InReview)
            .Build();
        var comments = "Please add more detailed description and fix formatting";

        // Act
        publication.RequestRevision(_moderatorId, comments);

        // Assert
        publication.Status.Should().Be(PublicationStatus.NeedsRevision);
        publication.ModeratorComments.Should().Be(comments);
        publication.ReviewedByUserId.Should().Be(_moderatorId);
        publication.ReviewedAtUtc.Should().NotBeNull();
        publication.ReviewedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(PublicationStatus.Draft)]
    [InlineData(PublicationStatus.Published)]
    [InlineData(PublicationStatus.NeedsRevision)]
    public void RequestRevision_FromInvalidStatus_ShouldThrowInvalidOperationException(PublicationStatus invalidStatus)
    {
        // Arrange
        var publication = TestDataBuilder.CreatePublication()
            .WithStatus(invalidStatus)
            .Build();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => publication.RequestRevision(_moderatorId, "comments"));
        exception.Message.Should().Contain($"Cannot request revision for publication with status {invalidStatus}");
    }

    [Theory]
    [InlineData(PublicationStatus.Draft)]
    [InlineData(PublicationStatus.NeedsRevision)]
    [InlineData(PublicationStatus.Rejected)]
    [InlineData(PublicationStatus.Published)]
    [InlineData(PublicationStatus.UnderReview)]
    public void Archive_FromValidStatus_ShouldUpdateStatusAndTimestamp(PublicationStatus validStatus)
    {
        // Arrange
        var publication = TestDataBuilder.CreatePublication()
            .WithStatus(validStatus)
            .Build();
        var originalUpdatedAt = publication.UpdatedAtUtc;

        // Act
        publication.Archive();

        // Assert
        publication.Status.Should().Be(PublicationStatus.Archived);
        publication.UpdatedAtUtc.Should().BeAfter(originalUpdatedAt);
    }

    [Theory]
    [InlineData(PublicationStatus.InReview)]
    [InlineData(PublicationStatus.Archived)]
    public void Archive_FromInvalidStatus_ShouldThrowInvalidOperationException(PublicationStatus invalidStatus)
    {
        // Arrange
        var publication = TestDataBuilder.CreatePublication()
            .WithStatus(invalidStatus)
            .Build();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => publication.Archive());
        exception.Message.Should().Contain($"Cannot archive publication with status {invalidStatus}");
    }

    [Fact]
    public void MarkUnderReview_FromPublishedStatus_ShouldUpdateStatusAndTimestamp()
    {
        // Arrange
        var publication = TestDataBuilder.CreatePublication()
            .WithStatus(PublicationStatus.Published)
            .Build();
        var originalUpdatedAt = publication.UpdatedAtUtc;

        // Act
        publication.MarkUnderReview();

        // Assert
        publication.Status.Should().Be(PublicationStatus.UnderReview);
        publication.UpdatedAtUtc.Should().BeAfter(originalUpdatedAt);
    }

    [Theory]
    [InlineData(PublicationStatus.Draft)]
    [InlineData(PublicationStatus.InReview)]
    [InlineData(PublicationStatus.Rejected)]
    [InlineData(PublicationStatus.Archived)]
    public void MarkUnderReview_FromInvalidStatus_ShouldThrowInvalidOperationException(PublicationStatus invalidStatus)
    {
        // Arrange
        var publication = TestDataBuilder.CreatePublication()
            .WithStatus(invalidStatus)
            .Build();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => publication.MarkUnderReview());
        exception.Message.Should().Contain($"Cannot mark publication as under review with status {invalidStatus}");
    }

    [Theory]
    [InlineData(PublicationStatus.Draft, PublicationStatus.InReview, true)]
    [InlineData(PublicationStatus.Draft, PublicationStatus.Archived, true)]
    [InlineData(PublicationStatus.InReview, PublicationStatus.Published, true)]
    [InlineData(PublicationStatus.InReview, PublicationStatus.Rejected, true)]
    [InlineData(PublicationStatus.InReview, PublicationStatus.NeedsRevision, true)]
    [InlineData(PublicationStatus.NeedsRevision, PublicationStatus.InReview, true)]
    [InlineData(PublicationStatus.Published, PublicationStatus.Archived, true)]
    [InlineData(PublicationStatus.Published, PublicationStatus.UnderReview, true)]
    [InlineData(PublicationStatus.UnderReview, PublicationStatus.Published, true)]
    [InlineData(PublicationStatus.Draft, PublicationStatus.Published, false)]
    [InlineData(PublicationStatus.Published, PublicationStatus.Draft, false)]
    [InlineData(PublicationStatus.Rejected, PublicationStatus.Published, false)]
    public void CanTransitionTo_ShouldReturnCorrectResult(PublicationStatus fromStatus, PublicationStatus toStatus, bool expectedResult)
    {
        // Arrange
        var publication = TestDataBuilder.CreatePublication()
            .WithStatus(fromStatus)
            .Build();

        // Act
        var result = publication.CanTransitionTo(toStatus);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(ContentRating.General)]
    [InlineData(ContentRating.Teen)]
    [InlineData(ContentRating.Mature)]
    [InlineData(ContentRating.Adult)]
    public void Approve_WithDifferentContentRatings_ShouldSetCorrectRating(ContentRating rating)
    {
        // Arrange
        var publication = TestDataBuilder.CreatePublication()
            .WithStatus(PublicationStatus.InReview)
            .Build();

        // Act
        publication.Approve(_moderatorId, rating, false);

        // Assert
        publication.ContentRating.Should().Be(rating);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Approve_WithNsfwFlag_ShouldSetCorrectNsfwStatus(bool isNsfw)
    {
        // Arrange
        var publication = TestDataBuilder.CreatePublication()
            .WithStatus(PublicationStatus.InReview)
            .Build();

        // Act
        publication.Approve(_moderatorId, ContentRating.General, isNsfw);

        // Assert
        publication.IsNsfw.Should().Be(isNsfw);
    }
}