using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using Mangalith.Application.Contracts.Files;
using Mangalith.Application.Validators;
using Moq;

namespace Mangalith.Tests.Application.Validators;

public class FileUploadRequestValidatorTests
{
    private readonly FileUploadRequestValidator _validator;

    public FileUploadRequestValidatorTests()
    {
        _validator = new FileUploadRequestValidator();
    }

    [Fact]
    public void Validate_WithValidRequest_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var mockFile = CreateMockFile("test.cbz", 1024);
        var request = new FileUploadRequest
        {
            File = mockFile,
            Title = "Test Manga",
            Description = "Test description"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithNullFile_ShouldHaveValidationError()
    {
        // Arrange
        var request = new FileUploadRequest
        {
            File = null!,
            Title = "Test Manga"
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.File)
              .WithErrorMessage("File is required");
    }

    [Fact]
    public void Validate_WithEmptyFileName_ShouldHaveValidationError()
    {
        // Arrange
        var mockFile = CreateMockFile("", 1024);
        var request = new FileUploadRequest
        {
            File = mockFile,
            Title = "Test Manga"
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.File.FileName)
              .WithErrorMessage("File name is required");
    }

    [Fact]
    public void Validate_WithZeroFileLength_ShouldHaveValidationError()
    {
        // Arrange
        var mockFile = CreateMockFile("test.cbz", 0);
        var request = new FileUploadRequest
        {
            File = mockFile,
            Title = "Test Manga"
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.File.Length)
              .WithErrorMessage("File cannot be empty");
    }

    [Fact]
    public void Validate_WithTitleTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var mockFile = CreateMockFile("test.cbz", 1024);
        var longTitle = new string('A', 201); // 201 characters
        var request = new FileUploadRequest
        {
            File = mockFile,
            Title = longTitle
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Title)
              .WithErrorMessage("Title cannot exceed 200 characters");
    }

    [Fact]
    public void Validate_WithDescriptionTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var mockFile = CreateMockFile("test.cbz", 1024);
        var longDescription = new string('A', 1001); // 1001 characters
        var request = new FileUploadRequest
        {
            File = mockFile,
            Description = longDescription
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Description)
              .WithErrorMessage("Description cannot exceed 1000 characters");
    }

    [Fact]
    public void Validate_WithValidTitleLength_ShouldNotHaveValidationError()
    {
        // Arrange
        var mockFile = CreateMockFile("test.cbz", 1024);
        var validTitle = new string('A', 200); // Exactly 200 characters
        var request = new FileUploadRequest
        {
            File = mockFile,
            Title = validTitle
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Validate_WithValidDescriptionLength_ShouldNotHaveValidationError()
    {
        // Arrange
        var mockFile = CreateMockFile("test.cbz", 1024);
        var validDescription = new string('A', 1000); // Exactly 1000 characters
        var request = new FileUploadRequest
        {
            File = mockFile,
            Description = validDescription
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_WithNullTitle_ShouldNotHaveValidationError()
    {
        // Arrange
        var mockFile = CreateMockFile("test.cbz", 1024);
        var request = new FileUploadRequest
        {
            File = mockFile,
            Title = null
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Validate_WithEmptyTitle_ShouldNotHaveValidationError()
    {
        // Arrange
        var mockFile = CreateMockFile("test.cbz", 1024);
        var request = new FileUploadRequest
        {
            File = mockFile,
            Title = ""
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Validate_WithNullDescription_ShouldNotHaveValidationError()
    {
        // Arrange
        var mockFile = CreateMockFile("test.cbz", 1024);
        var request = new FileUploadRequest
        {
            File = mockFile,
            Description = null
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_WithEmptyDescription_ShouldNotHaveValidationError()
    {
        // Arrange
        var mockFile = CreateMockFile("test.cbz", 1024);
        var request = new FileUploadRequest
        {
            File = mockFile,
            Description = ""
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Theory]
    [InlineData("test.cbz")]
    [InlineData("manga_chapter_1.zip")]
    [InlineData("volume_1.rar")]
    [InlineData("chapter.pdf")]
    public void Validate_WithValidFileNames_ShouldNotHaveValidationErrors(string fileName)
    {
        // Arrange
        var mockFile = CreateMockFile(fileName, 1024);
        var request = new FileUploadRequest
        {
            File = mockFile,
            Title = "Test Manga"
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.File.FileName);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(1024)]
    [InlineData(1048576)] // 1MB
    [InlineData(long.MaxValue)]
    public void Validate_WithValidFileSizes_ShouldNotHaveValidationErrors(long fileSize)
    {
        // Arrange
        var mockFile = CreateMockFile("test.cbz", fileSize);
        var request = new FileUploadRequest
        {
            File = mockFile,
            Title = "Test Manga"
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.File.Length);
    }

    [Fact]
    public void Validate_WithMinimalValidRequest_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var mockFile = CreateMockFile("test.cbz", 1);
        var request = new FileUploadRequest
        {
            File = mockFile
            // Title and Description are optional
        };

        // Act & Assert
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    private static IFormFile CreateMockFile(string fileName, long length)
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.Length).Returns(length);
        mockFile.Setup(f => f.ContentType).Returns("application/zip");
        return mockFile.Object;
    }
}