namespace Mangalith.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when publication validation fails
/// </summary>
public class PublicationValidationException : PublicationException
{
    public PublicationValidationException(string validationRule, string message) 
        : base("PUBLICATION_VALIDATION_FAILED", message)
    {
        ValidationRule = validationRule;
    }

    public PublicationValidationException(string validationRule, string message, Exception innerException) 
        : base("PUBLICATION_VALIDATION_FAILED", message, innerException)
    {
        ValidationRule = validationRule;
    }

    public string ValidationRule { get; }
}