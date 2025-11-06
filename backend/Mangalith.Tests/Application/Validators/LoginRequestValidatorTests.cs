using FluentAssertions;
using FluentValidation.TestHelper;
using Mangalith.Application.Contracts.Auth;
using Mangalith.Application.Validators;

namespace Mangalith.Tests.Application.Validators;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator;

    public LoginRequestValidatorTests()
    {
        _validator = new LoginRequestValidator();
    }

    [Fact]
    public void Validate_WithValidRequest_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "password123"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WithEmptyEmail_ShouldHaveValidationError(string email)
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = email,
            Password = "password123"
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("test@")]
    [InlineData("test.example.com")]
    public void Validate_WithInvalidEmailFormat_ShouldHaveValidationError(string email)
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = email,
            Password = "password123"
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_WithEmailTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var longEmail = new string('a', 250) + "@example.com"; // > 256 characters
        var request = new LoginRequest
        {
            Email = longEmail,
            Password = "password123"
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WithEmptyPassword_ShouldHaveValidationError(string password)
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = password
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_WithPasswordTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var longPassword = new string('a', 129); // > 128 characters
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = longPassword
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_WithValidLongEmail_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var longButValidEmail = new string('a', 240) + "@example.com"; // Exactly 252 characters (< 256)
        var request = new LoginRequest
        {
            Email = longButValidEmail,
            Password = "password123"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_WithValidLongPassword_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var longButValidPassword = new string('a', 128); // Exactly 128 characters
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = longButValidPassword
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Theory]
    [InlineData("user@domain.com")]
    [InlineData("test.email@example.org")]
    [InlineData("user+tag@domain.co.uk")]
    [InlineData("123@456.com")]
    public void Validate_WithValidEmailFormats_ShouldNotHaveValidationErrors(string email)
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = email,
            Password = "password123"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("a")]
    [InlineData("short")]
    [InlineData("password")]
    [InlineData("verylongpasswordthatisacceptable")]
    public void Validate_WithValidPasswordLengths_ShouldNotHaveValidationErrors(string password)
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = password
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_WithSpecialCharactersInEmail_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test+special.chars@sub-domain.example-site.com",
            Password = "password123"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithSpecialCharactersInPassword_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "p@ssw0rd!#$%^&*()"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }
}