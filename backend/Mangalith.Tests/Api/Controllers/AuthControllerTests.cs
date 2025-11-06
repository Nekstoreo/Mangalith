using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Mangalith.Api.Controllers;
using Mangalith.Application.Common.Exceptions;
using Mangalith.Application.Contracts.Auth;
using Mangalith.Application.Interfaces.Services;
using Moq;

namespace Mangalith.Tests.Api.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly AuthController _authController;

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _authController = new AuthController(_authServiceMock.Object);
    }

    [Fact]
    public async Task Register_WithValidRequest_ShouldReturnCreatedResult()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "password123",
            FullName = "John Doe"
        };

        var authResponse = new AuthResponse("jwt_token", DateTime.UtcNow.AddHours(1), "test@example.com", "John Doe");

        _authServiceMock
            .Setup(x => x.RegisterAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authResponse);

        // Act
        var result = await _authController.Register(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<CreatedResult>();
        var createdResult = result as CreatedResult;
        createdResult!.Location.Should().Be("api/profile/me");
        createdResult.Value.Should().Be(authResponse);

        _authServiceMock.Verify(x => x.RegisterAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Register_WhenServiceThrowsConflictException_ShouldPropagateException()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "existing@example.com",
            Password = "password123",
            FullName = "John Doe"
        };

        _authServiceMock
            .Setup(x => x.RegisterAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConflictAppException("Email already registered"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ConflictAppException>(
            () => _authController.Register(request, CancellationToken.None));

        exception.Message.Should().Be("Email already registered");
        _authServiceMock.Verify(x => x.RegisterAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnOkResult()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "password123"
        };

        var authResponse = new AuthResponse("jwt_token", DateTime.UtcNow.AddHours(1), "test@example.com", "Test User");

        _authServiceMock
            .Setup(x => x.LoginAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authResponse);

        // Act
        var result = await _authController.Login(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be(authResponse);

        _authServiceMock.Verify(x => x.LoginAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldPropagateUnauthorizedException()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "wrongpassword"
        };

        _authServiceMock
            .Setup(x => x.LoginAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAppException("Invalid credentials"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAppException>(
            () => _authController.Login(request, CancellationToken.None));

        exception.Message.Should().Be("Invalid credentials");
        _authServiceMock.Verify(x => x.LoginAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void TestPermission_ShouldReturnOkResult()
    {
        // Act
        var result = _authController.TestPermission();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(new { message = "You have the 'user.read' permission!" });
    }

    [Fact]
    public void TestRole_ShouldReturnOkResult()
    {
        // Act
        var result = _authController.TestRole();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(new { message = "You have at least Moderator role!" });
    }
}