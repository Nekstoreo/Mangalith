using FluentValidation;
using Mangalith.Application.Contracts.Files;

namespace Mangalith.Application.Validators;

public class FileUploadRequestValidator : AbstractValidator<FileUploadRequest>
{
    public FileUploadRequestValidator()
    {
        RuleFor(x => x.File)
            .NotNull()
            .WithMessage("File is required");

        RuleFor(x => x.File.FileName)
            .NotEmpty()
            .WithMessage("File name is required")
            .When(x => x.File != null);

        RuleFor(x => x.File.Length)
            .GreaterThan(0)
            .WithMessage("File cannot be empty")
            .When(x => x.File != null);

        RuleFor(x => x.Title)
            .MaximumLength(200)
            .WithMessage("Title cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.Title));

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Description cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}