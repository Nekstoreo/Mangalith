using FluentAssertions;
using Mangalith.Application.Common.Exceptions;
using Mangalith.Application.Interfaces.Repositories;
using Mangalith.Application.Interfaces.Services;
using Mangalith.Application.Services;
using Mangalith.Domain.Entities;
using Mangalith.Domain.Enums;
using Moq;

namespace Mangalith.Tests.Application.Services;

public class MangaServiceTests
{
    private readonly Mock<IMangaRepository> _mangaRepositoryMock;
    private readonly Mock<IPublicationService> _publicationServiceMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly MangaService _mangaService;
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _mangaId = Guid.NewGuid();

    public MangaServiceTests()
    {
        _mangaRepositoryMock = new Mock<IMangaRepository>();
        _publicationServiceMock = new Mock<IPublicationService>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _mangaService = new MangaService(_mangaRepositoryMock.Object, _publicationServiceMock.Object, _userRepositoryMock.Object);
    }

    [Fact]
    public async Task GetMangaByIdAsync_WithValidId_ShouldReturnManga()
    {
        // Arrange
        var manga = new Manga("Test Manga", _userId);
        _mangaRepositoryMock
            .Setup(x => x.GetByIdAsync(_mangaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(manga);

        // Act
        var result = await _mangaService.GetMangaByIdAsync(_mangaId);

        // Assert
        result.Should().Be(manga);
        _mangaRepositoryMock.Verify(x => x.GetByIdAsync(_mangaId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetMangaByIdAsync_WithNonExistentId_ShouldReturnNull()
    {
        // Arrange
        _mangaRepositoryMock
            .Setup(x => x.GetByIdAsync(_mangaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Manga?)null);

        // Act
        var result = await _mangaService.GetMangaByIdAsync(_mangaId);

        // Assert
        result.Should().BeNull();
        _mangaRepositoryMock.Verify(x => x.GetByIdAsync(_mangaId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPublicMangasAsync_ShouldReturnPublicMangas()
    {
        // Arrange
        var publicMangas = new List<Manga>
        {
            new("Public Manga 1", _userId),
            new("Public Manga 2", _userId)
        };

        _mangaRepositoryMock
            .Setup(x => x.GetPublicMangasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(publicMangas);

        // Act
        var result = await _mangaService.GetPublicMangasAsync();

        // Assert
        result.Should().BeEquivalentTo(publicMangas);
        _mangaRepositoryMock.Verify(x => x.GetPublicMangasAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SearchPublicMangasAsync_WithSearchTerm_ShouldReturnMatchingMangas()
    {
        // Arrange
        var searchTerm = "action";
        var searchResults = new List<Manga>
        {
            new("Action Manga 1", _userId),
            new("Action Manga 2", _userId)
        };

        _mangaRepositoryMock
            .Setup(x => x.SearchPublicMangasAsync(searchTerm, It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchResults);

        // Act
        var result = await _mangaService.SearchPublicMangasAsync(searchTerm);

        // Assert
        result.Should().BeEquivalentTo(searchResults);
        _mangaRepositoryMock.Verify(x => x.SearchPublicMangasAsync(searchTerm, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUserMangasAsync_WithUserId_ShouldReturnUserMangas()
    {
        // Arrange
        var userMangas = new List<Manga>
        {
            new("User Manga 1", _userId),
            new("User Manga 2", _userId)
        };

        _mangaRepositoryMock
            .Setup(x => x.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userMangas);

        // Act
        var result = await _mangaService.GetUserMangasAsync(_userId);

        // Assert
        result.Should().BeEquivalentTo(userMangas);
        _mangaRepositoryMock.Verify(x => x.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateMangaAsync_WithValidParameters_ShouldCreateMangaAndPublication()
    {
        // Arrange
        var title = "New Manga";
        var description = "New manga description";
        var user = new User("test@example.com", "hash", "Test User");

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _publicationServiceMock
            .Setup(x => x.CreatePublicationAsync(It.IsAny<Guid>(), _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Publication(Guid.NewGuid(), _userId));

        // Act
        var result = await _mangaService.CreateMangaAsync(title, description, _userId);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be(title);
        result.Description.Should().Be(description);
        result.CreatedByUserId.Should().Be(_userId);

        _userRepositoryMock.Verify(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()), Times.Once);
        _mangaRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Manga>(), It.IsAny<CancellationToken>()), Times.Once);
        _publicationServiceMock.Verify(x => x.CreatePublicationAsync(It.IsAny<Guid>(), _userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateMangaAsync_WithNonExistentUser_ShouldThrowNotFoundException()
    {
        // Arrange
        var title = "New Manga";
        var description = "New manga description";

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _mangaService.CreateMangaAsync(title, description, _userId));

        exception.Message.Should().Contain(_userId.ToString());
        exception.Message.Should().Contain("not found");

        _userRepositoryMock.Verify(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()), Times.Once);
        _mangaRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Manga>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateMangaAsync_WhenPublicationCreationFails_ShouldStillCreateManga()
    {
        // Arrange
        var title = "New Manga";
        var description = "New manga description";
        var user = new User("test@example.com", "hash", "Test User");

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _publicationServiceMock
            .Setup(x => x.CreatePublicationAsync(It.IsAny<Guid>(), _userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Publication creation failed"));

        // Act
        var result = await _mangaService.CreateMangaAsync(title, description, _userId);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be(title);
        result.Description.Should().Be(description);

        _mangaRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Manga>(), It.IsAny<CancellationToken>()), Times.Once);
        _publicationServiceMock.Verify(x => x.CreatePublicationAsync(It.IsAny<Guid>(), _userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateMangaAsync_WithValidParameters_ShouldUpdateManga()
    {
        // Arrange
        var manga = new Manga("Original Title", "Original Description", _userId);
        var newTitle = "Updated Title";
        var alternativeTitle = "Alt Title";
        var description = "Updated description";
        var author = "Author Name";
        var artist = "Artist Name";
        var year = 2023;

        _mangaRepositoryMock
            .Setup(x => x.GetByIdAsync(_mangaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(manga);

        // Act
        var result = await _mangaService.UpdateMangaAsync(_mangaId, newTitle, alternativeTitle, 
            description, author, artist, year, _userId);

        // Assert
        result.Should().Be(manga);
        result.Title.Should().Be(newTitle);
        result.AlternativeTitle.Should().Be(alternativeTitle);
        result.Description.Should().Be(description);
        result.Author.Should().Be(author);
        result.Artist.Should().Be(artist);
        result.Year.Should().Be(year);

        _mangaRepositoryMock.Verify(x => x.GetByIdAsync(_mangaId, It.IsAny<CancellationToken>()), Times.Once);
        _mangaRepositoryMock.Verify(x => x.UpdateAsync(manga, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateMangaAsync_WithNonExistentManga_ShouldThrowNotFoundException()
    {
        // Arrange
        _mangaRepositoryMock
            .Setup(x => x.GetByIdAsync(_mangaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Manga?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _mangaService.UpdateMangaAsync(_mangaId, "Title", null, null, null, null, null, _userId));

        exception.Message.Should().Contain(_mangaId.ToString());
        exception.Message.Should().Contain("not found");

        _mangaRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Manga>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateMangaAsync_WithDifferentUser_ShouldThrowForbiddenException()
    {
        // Arrange
        var differentUserId = Guid.NewGuid();
        var manga = new Manga("Original Title", _userId);

        _mangaRepositoryMock
            .Setup(x => x.GetByIdAsync(_mangaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(manga);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ForbiddenAppException>(
            () => _mangaService.UpdateMangaAsync(_mangaId, "Title", null, null, null, null, null, differentUserId));

        exception.Message.Should().Contain("only edit your own manga");

        _mangaRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Manga>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteMangaAsync_WithValidParameters_ShouldDeleteManga()
    {
        // Arrange
        var manga = new Manga("Test Manga", _userId);

        _mangaRepositoryMock
            .Setup(x => x.GetByIdAsync(_mangaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(manga);

        // Act
        var result = await _mangaService.DeleteMangaAsync(_mangaId, _userId);

        // Assert
        result.Should().BeTrue();
        _mangaRepositoryMock.Verify(x => x.GetByIdAsync(_mangaId, It.IsAny<CancellationToken>()), Times.Once);
        _mangaRepositoryMock.Verify(x => x.DeleteAsync(_mangaId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteMangaAsync_WithNonExistentManga_ShouldReturnFalse()
    {
        // Arrange
        _mangaRepositoryMock
            .Setup(x => x.GetByIdAsync(_mangaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Manga?)null);

        // Act
        var result = await _mangaService.DeleteMangaAsync(_mangaId, _userId);

        // Assert
        result.Should().BeFalse();
        _mangaRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteMangaAsync_WithDifferentUser_ShouldThrowForbiddenException()
    {
        // Arrange
        var differentUserId = Guid.NewGuid();
        var manga = new Manga("Test Manga", _userId);

        _mangaRepositoryMock
            .Setup(x => x.GetByIdAsync(_mangaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(manga);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ForbiddenAppException>(
            () => _mangaService.DeleteMangaAsync(_mangaId, differentUserId));

        exception.Message.Should().Contain("only delete your own manga");
        _mangaRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task IsMangaVisibleToUserAsync_WithCreatorUser_ShouldReturnTrue()
    {
        // Arrange
        var manga = new Manga("Test Manga", _userId);

        _mangaRepositoryMock
            .Setup(x => x.GetByIdAsync(_mangaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(manga);

        // Act
        var result = await _mangaService.IsMangaVisibleToUserAsync(_mangaId, _userId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsMangaVisibleToUserAsync_WithNonExistentManga_ShouldReturnFalse()
    {
        // Arrange
        _mangaRepositoryMock
            .Setup(x => x.GetByIdAsync(_mangaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Manga?)null);

        // Act
        var result = await _mangaService.IsMangaVisibleToUserAsync(_mangaId, _userId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsMangaVisibleToUserAsync_WithoutPublication_ShouldReturnFalseForNonCreator()
    {
        // Arrange
        var differentUserId = Guid.NewGuid();
        var manga = new Manga("Test Manga", _userId);

        _mangaRepositoryMock
            .Setup(x => x.GetByIdAsync(_mangaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(manga);

        // Act
        var result = await _mangaService.IsMangaVisibleToUserAsync(_mangaId, differentUserId);

        // Assert
        result.Should().BeFalse();
    }
}