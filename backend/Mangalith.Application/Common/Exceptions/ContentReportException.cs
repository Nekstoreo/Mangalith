namespace Mangalith.Application.Common.Exceptions;

public class ContentReportException : AppException
{
    public ContentReportException(string code, string message) 
        : base(code, message)
    {
    }

    public ContentReportException(string code, string message, Exception innerException) 
        : base(code, message, innerException)
    {
    }
}
