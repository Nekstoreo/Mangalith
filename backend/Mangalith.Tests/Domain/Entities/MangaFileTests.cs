using FluentAssertions;
using Mangalith.Domain.Entities;

namespace Mangalith.Tests.Domain.Entities;

public class MangaFileTests
{
    private readonly Guid _mangaId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateMangaFileWithCorrectProperties()
    {
        // Arrange
        var originalFileName = "manga_chapter_1.cbz";
        var storedFileName = "stored_file_123.cbz";
        var filePath = "/uploads/user123/stored_file_123.cbz";
        var fileSize = 1024000L;
        var mimeType = "application/zip";
        var fileType = MangaFileType.CBZ;
        var fileHash = "ABC123DEF456";

        // Act
        var mangaFile = new MangaFile(_mangaId, originalFileName, storedFileName, filePath, 
            fileSize, mimeType, fileType, _userId, fileHash);

        // Assert
        mangaFile.Id.Should().NotBe(Guid.Empty);
        mangaFile.MangaId.Should().Be(_mangaId);
        mangaFile.OriginalFileName.Should().Be(originalFileName);
        mangaFile.StoredFileName.Should().Be(storedFileName);
        mangaFile.FilePath.Should().Be(filePath);
        mangaFile.FileSize.Should().Be(fileSize);
        mangaFile.MimeType.Should().Be(mimeType);
        mangaFile.FileType.Should().Be(fileType);
        mangaFile.Status.Should().Be(MangaFileStatus.Uploaded);
        mangaFile.UploadedByUserId.Should().Be(_userId);
        mangaFile.FileHash.Should().Be(fileHash);
        mangaFile.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        mangaFile.UpdatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        mangaFile.ProcessingError.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithNullMangaId_ShouldCreateOrphanFile()
    {
        // Arrange
        var originalFileName = "orphan_file.cbz";
        var storedFileName = "stored_orphan_123.cbz";
        var filePath = "/uploads/user123/stored_orphan_123.cbz";
        var fileSize = 1024000L;
        var mimeType = "application/zip";
        var fileType = MangaFileType.CBZ;

        // Act
        var mangaFile = new MangaFile(null, originalFileName, storedFileName, filePath, 
            fileSize, mimeType, fileType, _userId);

        // Assert
        mangaFile.MangaId.Should().BeNull();
        mangaFile.OriginalFileName.Should().Be(originalFileName);
        mangaFile.Status.Should().Be(MangaFileStatus.Uploaded);
    }

    [Fact]
    public void Constructor_WithoutFileHash_ShouldCreateFileWithNullHash()
    {
        // Arrange & Act
        var mangaFile = new MangaFile(_mangaId, "test.cbz", "stored.cbz", "/path/stored.cbz", 
            1024L, "application/zip", MangaFileType.CBZ, _userId);

        // Assert
        mangaFile.FileHash.Should().BeNull();
    }

    [Fact]
    public void UpdateStatus_WithSuccessStatus_ShouldUpdateStatusAndTimestamp()
    {
        // Arrange
        var mangaFile = new MangaFile(_mangaId, "test.cbz", "stored.cbz", "/path/stored.cbz", 
            1024L, "application/zip", MangaFileType.CBZ, _userId);
        var originalUpdatedAt = mangaFile.UpdatedAtUtc;

        // Act
        mangaFile.UpdateStatus(MangaFileStatus.Processed);

        // Assert
        mangaFile.Status.Should().Be(MangaFileStatus.Processed);
        mangaFile.ProcessingError.Should().BeNull();
        mangaFile.UpdatedAtUtc.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void UpdateStatus_WithFailedStatus_ShouldUpdateStatusWithErrorAndTimestamp()
    {
        // Arrange
        var mangaFile = new MangaFile(_mangaId, "test.cbz", "stored.cbz", "/path/stored.cbz", 
            1024L, "application/zip", MangaFileType.CBZ, _userId);
        var originalUpdatedAt = mangaFile.UpdatedAtUtc;
        var errorMessage = "Failed to extract archive";

        // Act
        mangaFile.UpdateStatus(MangaFileStatus.Failed, errorMessage);

        // Assert
        mangaFile.Status.Should().Be(MangaFileStatus.Failed);
        mangaFile.ProcessingError.Should().Be(errorMessage);
        mangaFile.UpdatedAtUtc.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void UpdateStatus_WithProcessingStatus_ShouldClearPreviousError()
    {
        // Arrange
        var mangaFile = new MangaFile(_mangaId, "test.cbz", "stored.cbz", "/path/stored.cbz", 
            1024L, "application/zip", MangaFileType.CBZ, _userId);
        
        // First set failed status with error
        mangaFile.UpdateStatus(MangaFileStatus.Failed, "Previous error");
        
        // Act - Update to processing (should clear error)
        mangaFile.UpdateStatus(MangaFileStatus.Processing);

        // Assert
        mangaFile.Status.Should().Be(MangaFileStatus.Processing);
        mangaFile.ProcessingError.Should().BeNull();
    }

    [Fact]
    public void UpdateFileInfo_ShouldUpdateFileSizeAndHashAndTimestamp()
    {
        // Arrange
        var mangaFile = new MangaFile(_mangaId, "test.cbz", "stored.cbz", "/path/stored.cbz", 
            1024L, "application/zip", MangaFileType.CBZ, _userId);
        var originalUpdatedAt = mangaFile.UpdatedAtUtc;
        var newFileSize = 2048L;
        var newFileHash = "NEW123HASH456";

        // Act
        mangaFile.UpdateFileInfo(newFileSize, newFileHash);

        // Assert
        mangaFile.FileSize.Should().Be(newFileSize);
        mangaFile.FileHash.Should().Be(newFileHash);
        mangaFile.UpdatedAtUtc.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void UpdateFileInfo_WithoutHash_ShouldUpdateFileSizeAndSetHashToNull()
    {
        // Arrange
        var mangaFile = new MangaFile(_mangaId, "test.cbz", "stored.cbz", "/path/stored.cbz", 
            1024L, "application/zip", MangaFileType.CBZ, _userId, "ORIGINAL_HASH");
        var newFileSize = 2048L;

        // Act
        mangaFile.UpdateFileInfo(newFileSize);

        // Assert
        mangaFile.FileSize.Should().Be(newFileSize);
        mangaFile.FileHash.Should().BeNull(); // Default parameter value is null
    }

    [Theory]
    [InlineData(MangaFileType.CBZ)]
    [InlineData(MangaFileType.CBR)]
    [InlineData(MangaFileType.ZIP)]
    [InlineData(MangaFileType.RAR)]
    [InlineData(MangaFileType.PDF)]
    [InlineData(MangaFileType.Unknown)]
    public void Constructor_WithValidFileTypes_ShouldSetCorrectFileType(MangaFileType fileType)
    {
        // Arrange & Act
        var mangaFile = new MangaFile(_mangaId, "test.file", "stored.file", "/path/stored.file", 
            1024L, "application/octet-stream", fileType, _userId);

        // Assert
        mangaFile.FileType.Should().Be(fileType);
    }

    [Theory]
    [InlineData(MangaFileStatus.Uploaded)]
    [InlineData(MangaFileStatus.Processing)]
    [InlineData(MangaFileStatus.Processed)]
    [InlineData(MangaFileStatus.Failed)]
    [InlineData(MangaFileStatus.Archived)]
    public void UpdateStatus_WithValidStatuses_ShouldUpdateSuccessfully(MangaFileStatus status)
    {
        // Arrange
        var mangaFile = new MangaFile(_mangaId, "test.cbz", "stored.cbz", "/path/stored.cbz", 
            1024L, "application/zip", MangaFileType.CBZ, _userId);

        // Act
        mangaFile.UpdateStatus(status);

        // Assert
        mangaFile.Status.Should().Be(status);
    }

    [Fact]
    public void Constructor_WithLargeFileSize_ShouldHandleCorrectly()
    {
        // Arrange
        var largeFileSize = long.MaxValue;

        // Act
        var mangaFile = new MangaFile(_mangaId, "large.cbz", "stored_large.cbz", "/path/stored_large.cbz", 
            largeFileSize, "application/zip", MangaFileType.CBZ, _userId);

        // Assert
        mangaFile.FileSize.Should().Be(largeFileSize);
    }

    [Fact]
    public void Constructor_WithEmptyFileName_ShouldAcceptEmptyString()
    {
        // Arrange & Act
        var mangaFile = new MangaFile(_mangaId, "", "stored.cbz", "/path/stored.cbz", 
            1024L, "application/zip", MangaFileType.CBZ, _userId);

        // Assert
        mangaFile.OriginalFileName.Should().Be("");
    }
}