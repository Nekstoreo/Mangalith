using FluentAssertions;
using Mangalith.Application.Common.Exceptions;
using Mangalith.Application.Contracts.Auth;
using Mangalith.Application.Interfaces.Repositories;
using Mangalith.Application.Interfaces.Services;
using Mangalith.Application.Services;
using Mangalith.Domain.Entities;
using Mangalith.Tests.TestHelpers;
using Moq;

namespace Mangalith.Tests.Integration;

/// <summary>
/// Pruebas de integración que verifican el flujo completo de autenticación
/// sin dependencias externas reales
/// </summary>
public class AuthIntegrationTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtProvider> _jwtProviderMock;
    private readonly AuthService _authService;

    public AuthIntegrationTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtProviderMock = new Mock<IJwtProvider>();
        _authService = new AuthService(_userRepositoryMock.Object, _passwordHasherMock.Object, _jwtProviderMock.Object);
    }

    [Fact]
    public async Task CompleteRegistrationFlow_ShouldCreateUserAndReturnToken()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Email = "newuser@example.com",
            Password = "SecurePassword123",
            FullName = "New User"
        };

        var hashedPassword = "hashed_SecurePassword123";
        var expectedToken = new AuthResponse("jwt_token_12345", DateTime.UtcNow.AddHours(1), registerRequest.Email, registerRequest.FullName);

        // Setup mocks para simular el flujo completo
        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(registerRequest.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null); // Usuario no existe

        _passwordHasherMock
            .Setup(x => x.Hash(registerRequest.Password))
            .Returns(hashedPassword);

        _jwtProviderMock
            .Setup(x => x.CreateTokenAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedToken);

        // Act
        var result = await _authService.RegisterAsync(registerRequest);

        // Assert
        result.Should().Be(expectedToken);

        // Verificar que se llamaron todos los métodos en el orden correcto
        _userRepositoryMock.Verify(x => x.GetByEmailAsync(registerRequest.Email, It.IsAny<CancellationToken>()), Times.Once);
        _passwordHasherMock.Verify(x => x.Hash(registerRequest.Password), Times.Once);
        _userRepositoryMock.Verify(x => x.CreateAsync(It.Is<User>(u => 
            u.Email == registerRequest.Email && 
            u.PasswordHash == hashedPassword && 
            u.FullName == registerRequest.FullName &&
            u.Role == UserRole.Reader &&
            u.IsActive == true), It.IsAny<CancellationToken>()), Times.Once);
        _jwtProviderMock.Verify(x => x.CreateTokenAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CompleteLoginFlow_ShouldAuthenticateUserAndUpdateLastLogin()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "existing@example.com",
            Password = "UserPassword123"
        };

        var existingUser = TestDataBuilder.CreateUser()
            .WithEmail(loginRequest.Email)
            .WithPasswordHash("hashed_UserPassword123")
            .WithFullName("Existing User")
            .Build();

        var expectedToken = new AuthResponse("jwt_token_54321", DateTime.UtcNow.AddHours(1), loginRequest.Email, existingUser.FullName);

        var originalLastLogin = existingUser.LastLoginAtUtc;

        // Setup mocks
        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(loginRequest.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _passwordHasherMock
            .Setup(x => x.Verify(existingUser.PasswordHash, loginRequest.Password))
            .Returns(true);

        _jwtProviderMock
            .Setup(x => x.CreateTokenAsync(existingUser, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedToken);

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        result.Should().Be(expectedToken);
        existingUser.LastLoginAtUtc.Should().NotBe(originalLastLogin);
        existingUser.LastLoginAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        // Verificar flujo completo
        _userRepositoryMock.Verify(x => x.GetByEmailAsync(loginRequest.Email, It.IsAny<CancellationToken>()), Times.Once);
        _passwordHasherMock.Verify(x => x.Verify(existingUser.PasswordHash, loginRequest.Password), Times.Once);
        _jwtProviderMock.Verify(x => x.CreateTokenAsync(existingUser, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterExistingUser_ShouldThrowConflictException()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Email = "existing@example.com",
            Password = "Password123",
            FullName = "Duplicate User"
        };

        var existingUser = TestDataBuilder.CreateUser()
            .WithEmail(registerRequest.Email)
            .Build();

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(registerRequest.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ConflictAppException>(
            () => _authService.RegisterAsync(registerRequest));

        exception.Message.Should().Contain(registerRequest.Email);
        exception.Message.Should().Contain("already registered");

        // Verificar que no se intentó crear el usuario
        _passwordHasherMock.Verify(x => x.Hash(It.IsAny<string>()), Times.Never);
        _userRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _jwtProviderMock.Verify(x => x.CreateTokenAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LoginWithWrongPassword_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "user@example.com",
            Password = "WrongPassword123"
        };

        var existingUser = TestDataBuilder.CreateUser()
            .WithEmail(loginRequest.Email)
            .WithPasswordHash("hashed_CorrectPassword123")
            .Build();

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(loginRequest.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _passwordHasherMock
            .Setup(x => x.Verify(existingUser.PasswordHash, loginRequest.Password))
            .Returns(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAppException>(
            () => _authService.LoginAsync(loginRequest));

        exception.Message.Should().Be("Invalid credentials");

        // Verificar que no se generó token
        _jwtProviderMock.Verify(x => x.CreateTokenAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        
        // Verificar que no se actualizó el último login
        existingUser.LastLoginAtUtc.Should().BeNull();
    }

    [Fact]
    public async Task LoginNonExistentUser_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "Password123"
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(loginRequest.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAppException>(
            () => _authService.LoginAsync(loginRequest));

        exception.Message.Should().Be("Invalid credentials");

        // Verificar que no se verificó contraseña ni se generó token
        _passwordHasherMock.Verify(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _jwtProviderMock.Verify(x => x.CreateTokenAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAndLoginFlow_ShouldWorkSequentially()
    {
        // Arrange
        var email = "sequential@example.com";
        var password = "SequentialTest123";
        var hashedPassword = "hashed_SequentialTest123";

        var registerRequest = new RegisterRequest
        {
            Email = email,
            Password = password,
            FullName = "Sequential User"
        };

        var loginRequest = new LoginRequest
        {
            Email = email,
            Password = password
        };

        var authResponse = new AuthResponse("sequential_token", DateTime.UtcNow.AddHours(1), email, registerRequest.FullName);

        User? createdUser = null;

        // Setup para registro
        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _passwordHasherMock
            .Setup(x => x.Hash(password))
            .Returns(hashedPassword);

        _userRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((user, _) => createdUser = user)
            .ReturnsAsync((User user, CancellationToken _) => user);

        _jwtProviderMock
            .Setup(x => x.CreateTokenAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(authResponse);

        // Act - Registro
        var registerResult = await _authService.RegisterAsync(registerRequest);

        // Assert - Registro exitoso
        registerResult.Should().Be(authResponse);
        createdUser.Should().NotBeNull();

        // Setup para login (simular que ahora el usuario existe)
        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdUser);

        _passwordHasherMock
            .Setup(x => x.Verify(hashedPassword, password))
            .Returns(true);

        // Act - Login
        var loginResult = await _authService.LoginAsync(loginRequest);

        // Assert - Login exitoso
        loginResult.Should().Be(authResponse);
        createdUser!.LastLoginAtUtc.Should().NotBeNull();
        createdUser.LastLoginAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}