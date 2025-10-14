namespace Mangalith.Application.Common.Exceptions;

public abstract class AppException : Exception
{
    protected AppException(string code, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        Code = code;
    }

    public string Code { get; }
}
