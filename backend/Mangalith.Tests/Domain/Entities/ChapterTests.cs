using FluentAssertions;
using Mangalith.Domain.Entities;
using Mangalith.Tests.TestHelpers;

namespace Mangalith.Tests.Domain.Entities;

public class ChapterTests
{
    private readonly Guid _mangaId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateChapterWithCorrectProperties()
    {
        // Arrange
        var title = "Chapter 1: The Beginning";
        var number = 1.0;

        // Act
        var chapter = new Chapter(_mangaId, title, number, _userId);

        // Assert
        chapter.Id.Should().NotBe(Guid.Empty);
        chapter.MangaId.Should().Be(_mangaId);
        chapter.Title.Should().Be(title);
        chapter.Number.Should().Be(number);
        chapter.CreatedByUserId.Should().Be(_userId);
        chapter.VolumeNumber.Should().BeNull();
        chapter.PageCount.Should().Be(0);
        chapter.Notes.Should().BeNull();
        chapter.TranslatorNotes.Should().BeNull();
        chapter.Status.Should().Be(ChapterStatus.Draft);
        chapter.IsPublic.Should().BeFalse();
        chapter.PublishedAtUtc.Should().BeNull();
        chapter.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        chapter.UpdatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(1.0)]
    [InlineData(1.5)]
    [InlineData(2.1)]
    [InlineData(10.75)]
    public void Constructor_WithDecimalChapterNumbers_ShouldAcceptDecimalValues(double number)
    {
        // Act
        var chapter = new Chapter(_mangaId, "Test Chapter", number, _userId);

        // Assert
        chapter.Number.Should().Be(number);
    }

    [Fact]
    public void UpdateBasicInfo_ShouldUpdateAllFieldsAndTimestamp()
    {
        // Arrange
        var chapter = new Chapter(_mangaId, "Original Title", 1.0, _userId);
        var originalUpdatedAt = chapter.UpdatedAtUtc;
        
        var newTitle = "Updated Chapter Title";
        var newNumber = 1.5;
        var volumeNumber = 2;
        var notes = "Chapter notes";
        var translatorNotes = "Translator notes";

        // Act
        chapter.UpdateBasicInfo(newTitle, newNumber, volumeNumber, notes, translatorNotes);

        // Assert
        chapter.Title.Should().Be(newTitle);
        chapter.Number.Should().Be(newNumber);
        chapter.VolumeNumber.Should().Be(volumeNumber);
        chapter.Notes.Should().Be(notes);
        chapter.TranslatorNotes.Should().Be(translatorNotes);
        chapter.UpdatedAtUtc.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void UpdateBasicInfo_WithNullOptionalFields_ShouldAcceptNullValues()
    {
        // Arrange
        var chapter = new Chapter(_mangaId, "Test Chapter", 1.0, _userId);
        var newTitle = "Updated Title";
        var newNumber = 2.0;

        // Act
        chapter.UpdateBasicInfo(newTitle, newNumber, null, null, null);

        // Assert
        chapter.Title.Should().Be(newTitle);
        chapter.Number.Should().Be(newNumber);
        chapter.VolumeNumber.Should().BeNull();
        chapter.Notes.Should().BeNull();
        chapter.TranslatorNotes.Should().BeNull();
    }

    [Theory]
    [InlineData(ChapterStatus.Draft)]
    [InlineData(ChapterStatus.Processing)]
    [InlineData(ChapterStatus.Ready)]
    [InlineData(ChapterStatus.Published)]
    [InlineData(ChapterStatus.Archived)]
    public void UpdateStatus_WithValidStatus_ShouldUpdateStatusAndTimestamp(ChapterStatus status)
    {
        // Arrange
        var chapter = new Chapter(_mangaId, "Test Chapter", 1.0, _userId);
        var originalUpdatedAt = chapter.UpdatedAtUtc;

        // Act
        chapter.UpdateStatus(status);

        // Assert
        chapter.Status.Should().Be(status);
        chapter.UpdatedAtUtc.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void UpdateStatus_ToPublished_ShouldSetPublishedTimestamp()
    {
        // Arrange
        var chapter = new Chapter(_mangaId, "Test Chapter", 1.0, _userId);

        // Act
        chapter.UpdateStatus(ChapterStatus.Published);

        // Assert
        chapter.Status.Should().Be(ChapterStatus.Published);
        chapter.PublishedAtUtc.Should().NotBeNull();
        chapter.PublishedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void UpdateStatus_ToPublishedMultipleTimes_ShouldNotUpdatePublishedTimestamp()
    {
        // Arrange
        var chapter = new Chapter(_mangaId, "Test Chapter", 1.0, _userId);
        chapter.UpdateStatus(ChapterStatus.Published);
        var firstPublishedAt = chapter.PublishedAtUtc;

        // Act
        chapter.UpdateStatus(ChapterStatus.Ready);
        chapter.UpdateStatus(ChapterStatus.Published);

        // Assert
        chapter.PublishedAtUtc.Should().Be(firstPublishedAt);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SetPublic_ShouldUpdatePublicStatusAndTimestamp(bool isPublic)
    {
        // Arrange
        var chapter = new Chapter(_mangaId, "Test Chapter", 1.0, _userId);
        var originalUpdatedAt = chapter.UpdatedAtUtc;

        // Act
        chapter.SetPublic(isPublic);

        // Assert
        chapter.IsPublic.Should().Be(isPublic);
        chapter.UpdatedAtUtc.Should().BeAfter(originalUpdatedAt);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(25)]
    [InlineData(100)]
    public void UpdatePageCount_ShouldUpdateCountAndTimestamp(int pageCount)
    {
        // Arrange
        var chapter = new Chapter(_mangaId, "Test Chapter", 1.0, _userId);
        var originalUpdatedAt = chapter.UpdatedAtUtc;

        // Act
        chapter.UpdatePageCount(pageCount);

        // Assert
        chapter.PageCount.Should().Be(pageCount);
        chapter.UpdatedAtUtc.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void UpdateBasicInfo_WithCompleteInformation_ShouldUpdateAllFields()
    {
        // Arrange
        var chapter = new Chapter(_mangaId, "Test Chapter", 1.0, _userId);
        var title = "Chapter 5: The Final Battle";
        var number = 5.0;
        var volumeNumber = 1;
        var notes = "This is an important chapter";
        var translatorNotes = "Translation note: 'Nakama' means friend";

        // Act
        chapter.UpdateBasicInfo(title, number, volumeNumber, notes, translatorNotes);

        // Assert
        chapter.Title.Should().Be(title);
        chapter.Number.Should().Be(number);
        chapter.VolumeNumber.Should().Be(volumeNumber);
        chapter.Notes.Should().Be(notes);
        chapter.TranslatorNotes.Should().Be(translatorNotes);
    }

    [Fact]
    public void Chapter_WithBuilderPattern_ShouldCreateCorrectly()
    {
        // Arrange & Act
        var chapter = TestDataBuilder.CreateChapter()
            .WithMangaId(_mangaId)
            .WithTitle("Builder Chapter")
            .WithNumber(2.5)
            .WithCreatedByUserId(_userId)
            .WithVolumeNumber(3)
            .WithStatus(ChapterStatus.Ready)
            .WithPublic(true)
            .Build();

        // Assert
        chapter.MangaId.Should().Be(_mangaId);
        chapter.Title.Should().Be("Builder Chapter");
        chapter.Number.Should().Be(2.5);
        chapter.CreatedByUserId.Should().Be(_userId);
        chapter.VolumeNumber.Should().Be(3);
        chapter.Status.Should().Be(ChapterStatus.Ready);
        chapter.IsPublic.Should().BeTrue();
    }

    [Theory]
    [InlineData(0.1)]
    [InlineData(0.5)]
    [InlineData(999.99)]
    public void Constructor_WithEdgeCaseNumbers_ShouldAcceptValues(double number)
    {
        // Act
        var chapter = new Chapter(_mangaId, "Edge Case Chapter", number, _userId);

        // Assert
        chapter.Number.Should().Be(number);
    }

    [Fact]
    public void UpdateStatus_FromPublishedToArchived_ShouldMaintainPublishedTimestamp()
    {
        // Arrange
        var chapter = new Chapter(_mangaId, "Test Chapter", 1.0, _userId);
        chapter.UpdateStatus(ChapterStatus.Published);
        var publishedAt = chapter.PublishedAtUtc;

        // Act
        chapter.UpdateStatus(ChapterStatus.Archived);

        // Assert
        chapter.Status.Should().Be(ChapterStatus.Archived);
        chapter.PublishedAtUtc.Should().Be(publishedAt);
    }
}