using FluentAssertions;
using Mangalith.Application.Interfaces.Repositories;
using Mangalith.Domain.Entities;
using Moq;

namespace Mangalith.Tests.Application.Services;

public class FileServiceTests
{
    private readonly Mock<IMangaFileRepository> _mangaFileRepositoryMock;
    private readonly Guid _userId = Guid.NewGuid();

    public FileServiceTests()
    {
        _mangaFileRepositoryMock = new Mock<IMangaFileRepository>();
    }

    [Fact]
    public async Task GetMangaFileByIdAsync_WithValidId_ShouldReturnMangaFile()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var mangaFile = new MangaFile(Guid.NewGuid(), "test.cbz", "stored.cbz", 
            "/test/path/stored.cbz", 1024L, "application/zip", MangaFileType.CBZ, _userId);

        _mangaFileRepositoryMock
            .Setup(x => x.GetByIdAsync(fileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mangaFile);

        // Act
        var result = await _mangaFileRepositoryMock.Object.GetByIdAsync(fileId);

        // Assert
        result.Should().Be(mangaFile);
        _mangaFileRepositoryMock.Verify(x => x.GetByIdAsync(fileId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetMangaFileByIdAsync_WithNonExistentId_ShouldReturnNull()
    {
        // Arrange
        var fileId = Guid.NewGuid();

        _mangaFileRepositoryMock
            .Setup(x => x.GetByIdAsync(fileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MangaFile?)null);

        // Act
        var result = await _mangaFileRepositoryMock.Object.GetByIdAsync(fileId);

        // Assert
        result.Should().BeNull();
        _mangaFileRepositoryMock.Verify(x => x.GetByIdAsync(fileId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddMangaFileAsync_WithValidFile_ShouldAddSuccessfully()
    {
        // Arrange
        var mangaFile = new MangaFile(Guid.NewGuid(), "test.cbz", "stored.cbz", 
            "/test/path/stored.cbz", 1024L, "application/zip", MangaFileType.CBZ, _userId);

        // Act
        await _mangaFileRepositoryMock.Object.AddAsync(mangaFile);

        // Assert
        _mangaFileRepositoryMock.Verify(x => x.AddAsync(mangaFile, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteMangaFileAsync_WithValidId_ShouldDeleteSuccessfully()
    {
        // Arrange
        var fileId = Guid.NewGuid();

        // Act
        await _mangaFileRepositoryMock.Object.DeleteAsync(fileId);

        // Assert
        _mangaFileRepositoryMock.Verify(x => x.DeleteAsync(fileId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetMangaFileByHashAsync_WithValidHash_ShouldReturnMangaFile()
    {
        // Arrange
        var fileHash = "ABC123DEF456";
        var mangaFile = new MangaFile(Guid.NewGuid(), "test.cbz", "stored.cbz", 
            "/test/path/stored.cbz", 1024L, "application/zip", MangaFileType.CBZ, _userId, fileHash);

        _mangaFileRepositoryMock
            .Setup(x => x.GetByHashAsync(fileHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mangaFile);

        // Act
        var result = await _mangaFileRepositoryMock.Object.GetByHashAsync(fileHash);

        // Assert
        result.Should().Be(mangaFile);
        result!.FileHash.Should().Be(fileHash);
        _mangaFileRepositoryMock.Verify(x => x.GetByHashAsync(fileHash, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetMangaFileByHashAsync_WithNonExistentHash_ShouldReturnNull()
    {
        // Arrange
        var fileHash = "NONEXISTENT123";

        _mangaFileRepositoryMock
            .Setup(x => x.GetByHashAsync(fileHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MangaFile?)null);

        // Act
        var result = await _mangaFileRepositoryMock.Object.GetByHashAsync(fileHash);

        // Assert
        result.Should().BeNull();
        _mangaFileRepositoryMock.Verify(x => x.GetByHashAsync(fileHash, It.IsAny<CancellationToken>()), Times.Once);
    }
}

