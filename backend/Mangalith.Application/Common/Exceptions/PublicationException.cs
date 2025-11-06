namespace Mangalith.Application.Common.Exceptions;

public class PublicationException : AppException
{
    public PublicationException(string code, string message) 
        : base(code, message)
    {
    }

    public PublicationException(string code, string message, Exception innerException) 
        : base(code, message, innerException)
    {
    }
}
