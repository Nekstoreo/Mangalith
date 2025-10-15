namespace Mangalith.Application.Common.Exceptions;

public class ValidationAppException : AppException
{
    public ValidationAppException(IDictionary<string, string[]> failures)
        : base("validation_error", "One or more validation errors occurred.")
    {
        Failures = failures;
    }

    public IDictionary<string, string[]> Failures { get; }
}
