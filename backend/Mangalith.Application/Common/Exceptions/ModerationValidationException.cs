namespace Mangalith.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when moderation action validation fails
/// </summary>
public class ModerationValidationException : ModerationException
{
    public ModerationValidationException(string validationRule, string message) 
        : base("MODERATION_VALIDATION_FAILED", message)
    {
        ValidationRule = validationRule;
    }

    public ModerationValidationException(string validationRule, string message, Exception innerException) 
        : base("MODERATION_VALIDATION_FAILED", message, innerException)
    {
        ValidationRule = validationRule;
    }

    public string ValidationRule { get; }
}