namespace Mangalith.Application.Common.Exceptions;

public class ConflictAppException : AppException
{
    public ConflictAppException(string message)
        : base("conflict", message)
    {
    }
}
