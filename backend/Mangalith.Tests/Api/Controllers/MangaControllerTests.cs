using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Mangalith.Api.Controllers;
using Mangalith.Application.Common.Exceptions;
using Mangalith.Application.Interfaces.Services;
using Mangalith.Domain.Entities;
using Mangalith.Tests.TestHelpers;
using Moq;
using System.Security.Claims;

namespace Mangalith.Tests.Api.Controllers;

public class MangaControllerTests
{
    private readonly Mock<IMangaService> _mangaServiceMock;
    private readonly Mock<IPublicationService> _publicationServiceMock;
    private readonly Mock<ILogger<MangaController>> _loggerMock;
    private readonly MangaController _mangaController;
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _mangaId = Guid.NewGuid();

    public MangaControllerTests()
    {
        _mangaServiceMock = new Mock<IMangaService>();
        _publicationServiceMock = new Mock<IPublicationService>();
        _loggerMock = new Mock<ILogger<MangaController>>();
        
        _mangaController = new MangaController(
            _mangaServiceMock.Object,
            _publicationServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetPublicMangasAsync_WithValidRequest_ShouldReturnOkWithMangas()
    {
        // Arrange
        var mangas = new List<Manga>
        {
            TestDataBuilder.CreateManga().WithTitle("Public Manga 1").Build(),
            TestDataBuilder.CreateManga().WithTitle("Public Manga 2").Build()
        };

        _mangaServiceMock
            .Setup(x => x.GetPublicMangasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mangas);

        // Act
        var result = await _mangaController.GetPublicMangasAsync(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().NotBeNull();
        
        _mangaServiceMock.Verify(x => x.GetPublicMangasAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPublicMangasAsync_WhenServiceThrowsException_ShouldReturnBadRequest()
    {
        // Arrange
        _mangaServiceMock
            .Setup(x => x.GetPublicMangasAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _mangaController.GetPublicMangasAsync(CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().NotBeNull();
    }

    [Theory]
    [InlineData("action")]
    [InlineData("")]
    [InlineData(null)]
    public async Task SearchPublicMangasAsync_WithSearchTerm_ShouldReturnOkWithResults(string? searchTerm)
    {
        // Arrange
        var mangas = new List<Manga>
        {
            TestDataBuilder.CreateManga().WithTitle("Action Manga").Build()
        };

        _mangaServiceMock
            .Setup(x => x.SearchPublicMangasAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mangas);

        // Act
        var result = await _mangaController.SearchPublicMangasAsync(searchTerm, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _mangaServiceMock.Verify(x => x.SearchPublicMangasAsync(
            searchTerm ?? string.Empty, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetMangaByIdAsync_WithVisibleManga_ShouldReturnOkWithMangaDetails()
    {
        // Arrange
        SetupAnonymousUser();
        
        var manga = TestDataBuilder.CreateManga()
            .WithTitle("Test Manga")
            .WithCreatedByUserId(_userId)
            .Build();

        _mangaServiceMock
            .Setup(x => x.IsMangaVisibleToUserAsync(_mangaId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mangaServiceMock
            .Setup(x => x.GetMangaByIdAsync(_mangaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(manga);

        // Act
        var result = await _mangaController.GetMangaByIdAsync(_mangaId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().NotBeNull();
        
        _mangaServiceMock.Verify(x => x.IsMangaVisibleToUserAsync(_mangaId, null, It.IsAny<CancellationToken>()), Times.Once);
        _mangaServiceMock.Verify(x => x.GetMangaByIdAsync(_mangaId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetMangaByIdAsync_WithNonVisibleManga_ShouldReturnNotFound()
    {
        // Arrange
        SetupAnonymousUser();
        
        _mangaServiceMock
            .Setup(x => x.IsMangaVisibleToUserAsync(_mangaId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _mangaController.GetMangaByIdAsync(_mangaId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        _mangaServiceMock.Verify(x => x.GetMangaByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetMangaByIdAsync_WithNonExistentManga_ShouldReturnNotFound()
    {
        // Arrange
        SetupAnonymousUser();
        
        _mangaServiceMock
            .Setup(x => x.IsMangaVisibleToUserAsync(_mangaId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mangaServiceMock
            .Setup(x => x.GetMangaByIdAsync(_mangaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Manga?)null);

        // Act
        var result = await _mangaController.GetMangaByIdAsync(_mangaId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetMyMangasAsync_WithAuthenticatedUser_ShouldReturnOkWithUserMangas()
    {
        // Arrange
        SetupAuthenticatedUser();
        var userMangas = new List<Manga>
        {
            TestDataBuilder.CreateManga().WithCreatedByUserId(_userId).WithTitle("My Manga 1").Build(),
            TestDataBuilder.CreateManga().WithCreatedByUserId(_userId).WithTitle("My Manga 2").Build()
        };

        _mangaServiceMock
            .Setup(x => x.GetUserMangasAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userMangas);

        // Act
        var result = await _mangaController.GetMyMangasAsync(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _mangaServiceMock.Verify(x => x.GetUserMangasAsync(_userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateMangaAsync_WithValidRequest_ShouldReturnCreatedResult()
    {
        // Arrange
        SetupAuthenticatedUser();
        var request = new CreateMangaRequest
        {
            Title = "New Manga",
            Description = "New manga description"
        };
        var createdManga = TestDataBuilder.CreateManga()
            .WithTitle(request.Title)
            .WithDescription(request.Description)
            .WithCreatedByUserId(_userId)
            .Build();

        _mangaServiceMock
            .Setup(x => x.CreateMangaAsync(request.Title, request.Description, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdManga);

        // Act
        var result = await _mangaController.CreateMangaAsync(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result as CreatedAtActionResult;
        createdResult!.ActionName.Should().Be(nameof(MangaController.GetMangaByIdAsync));
        createdResult.RouteValues!["id"].Should().Be(createdManga.Id);
        
        _mangaServiceMock.Verify(x => x.CreateMangaAsync(
            request.Title, request.Description, _userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateMangaAsync_WhenServiceThrowsException_ShouldReturnBadRequest()
    {
        // Arrange
        SetupAuthenticatedUser();
        var request = new CreateMangaRequest { Title = "Test Manga" };

        _mangaServiceMock
            .Setup(x => x.CreateMangaAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Creation failed"));

        // Act
        var result = await _mangaController.CreateMangaAsync(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UpdateMangaAsync_WithValidRequest_ShouldReturnOkWithUpdatedManga()
    {
        // Arrange
        SetupAuthenticatedUser();
        var request = new UpdateMangaRequest
        {
            Title = "Updated Title",
            AlternativeTitle = "Alt Title",
            Description = "Updated description",
            Author = "Author Name",
            Artist = "Artist Name",
            Year = 2023
        };
        var updatedManga = TestDataBuilder.CreateManga()
            .WithTitle(request.Title)
            .WithDescription(request.Description)
            .Build();

        _mangaServiceMock
            .Setup(x => x.UpdateMangaAsync(_mangaId, request.Title, request.AlternativeTitle, 
                request.Description, request.Author, request.Artist, request.Year, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedManga);

        // Act
        var result = await _mangaController.UpdateMangaAsync(_mangaId, request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _mangaServiceMock.Verify(x => x.UpdateMangaAsync(_mangaId, request.Title, request.AlternativeTitle,
            request.Description, request.Author, request.Artist, request.Year, _userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateMangaAsync_WhenServiceThrowsForbiddenException_ShouldReturnBadRequest()
    {
        // Arrange
        SetupAuthenticatedUser();
        var request = new UpdateMangaRequest { Title = "Updated Title" };

        _mangaServiceMock
            .Setup(x => x.UpdateMangaAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ForbiddenAppException("You can only edit your own manga"));

        // Act
        var result = await _mangaController.UpdateMangaAsync(_mangaId, request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task DeleteMangaAsync_WithValidRequest_ShouldReturnNoContent()
    {
        // Arrange
        SetupAuthenticatedUser();

        _mangaServiceMock
            .Setup(x => x.DeleteMangaAsync(_mangaId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _mangaController.DeleteMangaAsync(_mangaId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _mangaServiceMock.Verify(x => x.DeleteMangaAsync(_mangaId, _userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteMangaAsync_WithNonExistentManga_ShouldReturnNotFound()
    {
        // Arrange
        SetupAuthenticatedUser();

        _mangaServiceMock
            .Setup(x => x.DeleteMangaAsync(_mangaId, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _mangaController.DeleteMangaAsync(_mangaId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task DeleteMangaAsync_WhenServiceThrowsForbiddenException_ShouldReturnBadRequest()
    {
        // Arrange
        SetupAuthenticatedUser();

        _mangaServiceMock
            .Setup(x => x.DeleteMangaAsync(_mangaId, _userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ForbiddenAppException("You can only delete your own manga"));

        // Act
        var result = await _mangaController.DeleteMangaAsync(_mangaId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetMyMangasAsync_WhenServiceThrowsException_ShouldReturnBadRequest()
    {
        // Arrange
        SetupAuthenticatedUser();

        _mangaServiceMock
            .Setup(x => x.GetUserMangasAsync(_userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _mangaController.GetMyMangasAsync(CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task SearchPublicMangasAsync_WhenServiceThrowsException_ShouldReturnBadRequest()
    {
        // Arrange
        _mangaServiceMock
            .Setup(x => x.SearchPublicMangasAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Search error"));

        // Act
        var result = await _mangaController.SearchPublicMangasAsync("test", CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetMangaByIdAsync_WhenServiceThrowsException_ShouldReturnBadRequest()
    {
        // Arrange
        SetupAnonymousUser();
        
        _mangaServiceMock
            .Setup(x => x.IsMangaVisibleToUserAsync(_mangaId, null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Visibility check error"));

        // Act
        var result = await _mangaController.GetMangaByIdAsync(_mangaId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    private void SetupAuthenticatedUser()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, _userId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        
        _mangaController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };
    }

    private void SetupAnonymousUser()
    {
        // Configure an anonymous user with empty claims
        var identity = new ClaimsIdentity();
        var principal = new ClaimsPrincipal(identity);
        
        _mangaController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };
    }
}