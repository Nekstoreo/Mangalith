using FluentAssertions;
using Mangalith.Application.Common.Exceptions;
using Mangalith.Application.Contracts.Auth;
using Mangalith.Application.Interfaces.Repositories;
using Mangalith.Application.Interfaces.Services;
using Mangalith.Application.Services;
using Mangalith.Domain.Entities;
using Moq;

namespace Mangalith.Tests.Application.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtProvider> _jwtProviderMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtProviderMock = new Mock<IJwtProvider>();
        _authService = new AuthService(_userRepositoryMock.Object, _passwordHasherMock.Object, _jwtProviderMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_WithValidRequest_ShouldCreateUserAndReturnAuthResponse()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "password123",
            FullName = "John Doe"
        };

        var hashedPassword = "hashedPassword123";
        var authResponse = new AuthResponse("jwt_token", DateTime.UtcNow.AddHours(1), request.Email, request.FullName);

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _passwordHasherMock
            .Setup(x => x.Hash(request.Password))
            .Returns(hashedPassword);

        _jwtProviderMock
            .Setup(x => x.CreateTokenAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(authResponse);

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        result.Should().Be(authResponse);
        
        _userRepositoryMock.Verify(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()), Times.Once);
        _passwordHasherMock.Verify(x => x.Hash(request.Password), Times.Once);
        _userRepositoryMock.Verify(x => x.CreateAsync(It.Is<User>(u => 
            u.Email == request.Email && 
            u.PasswordHash == hashedPassword && 
            u.FullName == request.FullName), It.IsAny<CancellationToken>()), Times.Once);
        _jwtProviderMock.Verify(x => x.CreateTokenAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ShouldThrowConflictException()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "existing@example.com",
            Password = "password123",
            FullName = "John Doe"
        };

        var existingUser = new User(request.Email, "existingHash", "Existing User");

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ConflictAppException>(
            () => _authService.RegisterAsync(request));

        exception.Message.Should().Contain(request.Email);
        exception.Message.Should().Contain("already registered");

        _userRepositoryMock.Verify(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()), Times.Once);
        _passwordHasherMock.Verify(x => x.Hash(It.IsAny<string>()), Times.Never);
        _userRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnAuthResponseAndUpdateLastLogin()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "password123"
        };

        var user = new User(request.Email, "hashedPassword", "John Doe");
        var authResponse = new AuthResponse("jwt_token", DateTime.UtcNow.AddHours(1), request.Email, user.FullName);

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify(user.PasswordHash, request.Password))
            .Returns(true);

        _jwtProviderMock
            .Setup(x => x.CreateTokenAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authResponse);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.Should().Be(authResponse);
        user.LastLoginAtUtc.Should().NotBeNull();
        user.LastLoginAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        _userRepositoryMock.Verify(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()), Times.Once);
        _passwordHasherMock.Verify(x => x.Verify(user.PasswordHash, request.Password), Times.Once);
        _jwtProviderMock.Verify(x => x.CreateTokenAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentUser_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "password123"
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAppException>(
            () => _authService.LoginAsync(request));

        exception.Message.Should().Be("Invalid credentials");

        _userRepositoryMock.Verify(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()), Times.Once);
        _passwordHasherMock.Verify(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _jwtProviderMock.Verify(x => x.CreateTokenAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "wrongpassword"
        };

        var user = new User(request.Email, "hashedPassword", "John Doe");

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify(user.PasswordHash, request.Password))
            .Returns(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAppException>(
            () => _authService.LoginAsync(request));

        exception.Message.Should().Be("Invalid credentials");

        _userRepositoryMock.Verify(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()), Times.Once);
        _passwordHasherMock.Verify(x => x.Verify(user.PasswordHash, request.Password), Times.Once);
        _jwtProviderMock.Verify(x => x.CreateTokenAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}