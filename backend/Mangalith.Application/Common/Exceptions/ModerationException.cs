namespace Mangalith.Application.Common.Exceptions;

public class ModerationException : AppException
{
    public ModerationException(string code, string message) 
        : base(code, message)
    {
    }

    public ModerationException(string code, string message, Exception innerException) 
        : base(code, message, innerException)
    {
    }
}
