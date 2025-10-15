namespace Mangalith.Application.Common.Exceptions;

public class NotFoundException : AppException
{
    public NotFoundException(string message) 
        : base("NOT_FOUND", message)
    {
    }

    public NotFoundException(string message, Exception innerException) 
        : base("NOT_FOUND", message, innerException)
    {
    }
}
