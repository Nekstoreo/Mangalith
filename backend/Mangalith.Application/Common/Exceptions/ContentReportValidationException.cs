namespace Mangalith.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when content report validation fails
/// </summary>
public class ContentReportValidationException : ContentReportException
{
    public ContentReportValidationException(string validationRule, string message) 
        : base("CONTENT_REPORT_VALIDATION_FAILED", message)
    {
        ValidationRule = validationRule;
    }

    public ContentReportValidationException(string validationRule, string message, Exception innerException) 
        : base("CONTENT_REPORT_VALIDATION_FAILED", message, innerException)
    {
        ValidationRule = validationRule;
    }

    public string ValidationRule { get; }
}