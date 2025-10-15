namespace Mangalith.Application.Common.Exceptions;

public class UnauthorizedAppException : AppException
{
    public UnauthorizedAppException(string message = "Unauthorized access")
        : base("unauthorized", message)
    {
    }
}
