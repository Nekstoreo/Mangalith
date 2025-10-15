namespace Mangalith.Application.Common.Exceptions;

public class FileUploadException : Exception
{
    public FileUploadException(string message) : base(message) { }
    public FileUploadException(string message, Exception innerException) : base(message, innerException) { }
}

public class InvalidFileTypeException : FileUploadException
{
    public InvalidFileTypeException(string fileName, string[] allowedTypes) 
        : base($"File '{fileName}' has invalid type. Allowed types: {string.Join(", ", allowedTypes)}") { }
}

public class FileSizeExceededException : FileUploadException
{
    public FileSizeExceededException(string fileName, long fileSize, long maxSize) 
        : base($"File '{fileName}' size ({fileSize} bytes) exceeds maximum allowed size ({maxSize} bytes)") { }
}

public class FileProcessingException : FileUploadException
{
    public FileProcessingException(string fileName, string reason) 
        : base($"Failed to process file '{fileName}': {reason}") { }
}