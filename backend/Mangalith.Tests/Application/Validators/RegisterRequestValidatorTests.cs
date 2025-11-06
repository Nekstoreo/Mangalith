using FluentAssertions;
using FluentValidation.TestHelper;
using Mangalith.Application.Contracts.Auth;
using Mangalith.Application.Validators;

namespace Mangalith.Tests.Application.Validators;

public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator;

    public RegisterRequestValidatorTests()
    {
        _validator = new RegisterRequestValidator();
    }

    [Fact]
    public void Validate_WithValidRequest_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "Password123",
            ConfirmPassword = "Password123",
            FullName = "John Doe"
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
    public void Validate_WithInvalidEmail_ShouldHaveValidationError(string email)
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = email,
            Password = "Password123",
            ConfirmPassword = "Password123",
            FullName = "John Doe"
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
        var request = new RegisterRequest
        {
            Email = email,
            Password = "Password123",
            ConfirmPassword = "Password123",
            FullName = "John Doe"
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
        var request = new RegisterRequest
        {
            Email = longEmail,
            Password = "Password123",
            ConfirmPassword = "Password123",
            FullName = "John Doe"
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
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = password,
            ConfirmPassword = password,
            FullName = "John Doe"
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Theory]
    [InlineData("short")]
    [InlineData("1234567")]
    public void Validate_WithPasswordTooShort_ShouldHaveValidationError(string password)
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = password,
            ConfirmPassword = password,
            FullName = "John Doe"
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_WithPasswordTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var longPassword = new string('A', 129); // > 128 characters
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = longPassword,
            ConfirmPassword = longPassword,
            FullName = "John Doe"
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Theory]
    [InlineData("password123")] // No uppercase
    [InlineData("PASSWORD123")] // No lowercase
    [InlineData("PasswordABC")] // No digit
    public void Validate_WithPasswordMissingRequiredCharacters_ShouldHaveValidationError(string password)
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = password,
            ConfirmPassword = password,
            FullName = "John Doe"
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_WithMismatchedPasswords_ShouldHaveValidationError()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "Password123",
            ConfirmPassword = "DifferentPassword123",
            FullName = "John Doe"
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword)
              .WithErrorMessage("Passwords do not match.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WithEmptyFullName_ShouldHaveValidationError(string fullName)
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "Password123",
            ConfirmPassword = "Password123",
            FullName = fullName
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void Validate_WithFullNameTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var longFullName = new string('A', 201); // > 200 characters
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "Password123",
            ConfirmPassword = "Password123",
            FullName = longFullName
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void Validate_WithValidComplexPassword_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "MyComplexPassword123!",
            ConfirmPassword = "MyComplexPassword123!",
            FullName = "John Doe"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_WithValidLongFullName_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var longButValidFullName = new string('A', 200); // Exactly 200 characters
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "Password123",
            ConfirmPassword = "Password123",
            FullName = longButValidFullName
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void Validate_WithValidLongEmail_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var longButValidEmail = new string('a', 240) + "@example.com"; // Exactly 252 characters (< 256)
        var request = new RegisterRequest
        {
            Email = longButValidEmail,
            Password = "Password123",
            ConfirmPassword = "Password123",
            FullName = "John Doe"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }
}