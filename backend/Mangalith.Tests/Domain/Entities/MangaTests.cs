using FluentAssertions;
using Mangalith.Domain.Entities;
using Mangalith.Domain.Enums;

namespace Mangalith.Tests.Domain.Entities;

public class MangaTests
{
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public void Constructor_WithTitleAndUserId_ShouldCreateMangaWithCorrectProperties()
    {
        // Arrange
        var title = "Test Manga";

        // Act
        var manga = new Manga(title, _userId);

        // Assert
        manga.Id.Should().NotBe(Guid.Empty);
        manga.Title.Should().Be(title);
        manga.CreatedByUserId.Should().Be(_userId);
        manga.Status.Should().Be(MangaStatus.Ongoing);
        manga.IsPublic.Should().BeFalse();
        manga.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        manga.UpdatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Constructor_WithTitleDescriptionAndUserId_ShouldCreateMangaWithDraftStatus()
    {
        // Arrange
        var title = "Test Manga";
        var description = "Test description";

        // Act
        var manga = new Manga(title, description, _userId);

        // Assert
        manga.Title.Should().Be(title);
        manga.Description.Should().Be(description);
        manga.Status.Should().Be(MangaStatus.Draft);
        manga.IsPublic.Should().BeFalse();
    }

    [Fact]
    public void UpdateBasicInfo_ShouldUpdateAllFieldsAndTimestamp()
    {
        // Arrange
        var manga = new Manga("Original Title", _userId);
        var originalUpdatedAt = manga.UpdatedAtUtc;
        
        var newTitle = "Updated Title";
        var alternativeTitle = "Alt Title";
        var description = "New description";
        var author = "Author Name";
        var artist = "Artist Name";
        var year = 2023;

        // Act
        manga.UpdateBasicInfo(newTitle, alternativeTitle, description, author, artist, year);

        // Assert
        manga.Title.Should().Be(newTitle);
        manga.AlternativeTitle.Should().Be(alternativeTitle);
        manga.Description.Should().Be(description);
        manga.Author.Should().Be(author);
        manga.Artist.Should().Be(artist);
        manga.Year.Should().Be(year);
        manga.UpdatedAtUtc.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void UpdateStatus_ShouldUpdateStatusAndTimestamp()
    {
        // Arrange
        var manga = new Manga("Test Manga", _userId);
        var originalUpdatedAt = manga.UpdatedAtUtc;

        // Act
        manga.UpdateStatus(MangaStatus.Completed);

        // Assert
        manga.Status.Should().Be(MangaStatus.Completed);
        manga.UpdatedAtUtc.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void UpdateCoverImage_ShouldUpdateCoverImagePathAndTimestamp()
    {
        // Arrange
        var manga = new Manga("Test Manga", _userId);
        var coverImagePath = "/uploads/covers/manga123.jpg";
        var originalUpdatedAt = manga.UpdatedAtUtc;

        // Act
        manga.UpdateCoverImage(coverImagePath);

        // Assert
        manga.CoverImagePath.Should().Be(coverImagePath);
        manga.UpdatedAtUtc.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void UpdateTags_ShouldUpdateTagsAndTimestamp()
    {
        // Arrange
        var manga = new Manga("Test Manga", _userId);
        var tags = "[\"action\", \"adventure\", \"fantasy\"]";
        var originalUpdatedAt = manga.UpdatedAtUtc;

        // Act
        manga.UpdateTags(tags);

        // Assert
        manga.Tags.Should().Be(tags);
        manga.UpdatedAtUtc.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void UpdateGenres_ShouldUpdateGenresAndTimestamp()
    {
        // Arrange
        var manga = new Manga("Test Manga", _userId);
        var genres = "[\"shounen\", \"action\"]";
        var originalUpdatedAt = manga.UpdatedAtUtc;

        // Act
        manga.UpdateGenres(genres);

        // Assert
        manga.Genres.Should().Be(genres);
        manga.UpdatedAtUtc.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void IncrementViewCount_ShouldIncreaseViewCountAndUpdateTimestamp()
    {
        // Arrange
        var manga = new Manga("Test Manga", _userId);
        var originalViewCount = manga.ViewCount;
        var originalUpdatedAt = manga.UpdatedAtUtc;

        // Act
        manga.IncrementViewCount();

        // Assert
        manga.ViewCount.Should().Be(originalViewCount + 1);
        manga.UpdatedAtUtc.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void UpdateRating_ShouldUpdateRatingAndRatingCountAndTimestamp()
    {
        // Arrange
        var manga = new Manga("Test Manga", _userId);
        var newRating = 4.5;
        var newRatingCount = 10;
        var originalUpdatedAt = manga.UpdatedAtUtc;

        // Act
        manga.UpdateRating(newRating, newRatingCount);

        // Assert
        manga.Rating.Should().Be(newRating);
        manga.RatingCount.Should().Be(newRatingCount);
        manga.UpdatedAtUtc.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void SetPublic_ShouldUpdatePublicStatusAndTimestamp()
    {
        // Arrange
        var manga = new Manga("Test Manga", _userId);
        var originalUpdatedAt = manga.UpdatedAtUtc;

        // Act
        manga.SetPublic(true);

        // Assert
        manga.IsPublic.Should().BeTrue();
        manga.UpdatedAtUtc.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void UpdateChapterCount_ShouldUpdateCountAndTimestamp()
    {
        // Arrange
        var manga = new Manga("Test Manga", _userId);
        var chapterCount = 25;
        var originalUpdatedAt = manga.UpdatedAtUtc;

        // Act
        manga.UpdateChapterCount(chapterCount);

        // Assert
        manga.ChapterCount.Should().Be(chapterCount);
        manga.UpdatedAtUtc.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void IsVisibleToPublic_WithoutPublication_ShouldReturnIsPublicValue()
    {
        // Arrange
        var manga = new Manga("Test Manga", _userId);
        
        // Act & Assert - Initially false
        manga.IsVisibleToPublic().Should().BeFalse();
        
        // Set public and test again
        manga.SetPublic(true);
        manga.IsVisibleToPublic().Should().BeTrue();
    }

    [Fact]
    public void IsVisibleToUser_WithCreatorUserId_ShouldReturnTrue()
    {
        // Arrange
        var manga = new Manga("Test Manga", _userId);

        // Act & Assert
        manga.IsVisibleToUser(_userId).Should().BeTrue();
    }

    [Fact]
    public void IsVisibleToUser_WithDifferentUserId_ShouldReturnPublicVisibility()
    {
        // Arrange
        var manga = new Manga("Test Manga", _userId);
        var otherUserId = Guid.NewGuid();

        // Act & Assert - Initially false (not public)
        manga.IsVisibleToUser(otherUserId).Should().BeFalse();
        
        // Set public and test again
        manga.SetPublic(true);
        manga.IsVisibleToUser(otherUserId).Should().BeTrue();
    }

    [Fact]
    public void IsVisibleToUser_WithNullUserId_ShouldReturnPublicVisibility()
    {
        // Arrange
        var manga = new Manga("Test Manga", _userId);

        // Act & Assert - Initially false (not public)
        manga.IsVisibleToUser(null).Should().BeFalse();
        
        // Set public and test again
        manga.SetPublic(true);
        manga.IsVisibleToUser(null).Should().BeTrue();
    }

    [Theory]
    [InlineData(MangaStatus.Unknown)]
    [InlineData(MangaStatus.Ongoing)]
    [InlineData(MangaStatus.Completed)]
    [InlineData(MangaStatus.Hiatus)]
    [InlineData(MangaStatus.Cancelled)]
    [InlineData(MangaStatus.Draft)]
    public void UpdateStatus_WithValidStatuses_ShouldUpdateSuccessfully(MangaStatus status)
    {
        // Arrange
        var manga = new Manga("Test Manga", _userId);

        // Act
        manga.UpdateStatus(status);

        // Assert
        manga.Status.Should().Be(status);
    }
}