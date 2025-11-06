namespace Mangalith.Tests.TestHelpers;

public class MockQuotaExceededException : Exception
{
    public string ResourceType { get; }
    public long RequestedAmount { get; }
    public long AvailableAmount { get; }

    public MockQuotaExceededException(string resourceType, long requestedAmount, long availableAmount)
        : base($"Quota exceeded for {resourceType}. Requested: {requestedAmount}, Available: {availableAmount}")
    {
        ResourceType = resourceType;
        RequestedAmount = requestedAmount;
        AvailableAmount = availableAmount;
    }
}

public class MockFileUploadException : Exception
{
    public MockFileUploadException(string message) : base(message) { }
    public MockFileUploadException(string message, Exception innerException) : base(message, innerException) { }
}

public class MockFileSizeExceededException : MockFileUploadException
{
    public string FileName { get; }
    public long FileSize { get; }
    public long MaxAllowedSize { get; }

    public MockFileSizeExceededException(string fileName, long fileSize, long maxAllowedSize)
        : base($"File '{fileName}' size {fileSize} exceeds maximum allowed size {maxAllowedSize}")
    {
        FileName = fileName;
        FileSize = fileSize;
        MaxAllowedSize = maxAllowedSize;
    }
}

public class MockInvalidFileTypeException : MockFileUploadException
{
    public string FileName { get; }
    public string[] AllowedExtensions { get; }

    public MockInvalidFileTypeException(string fileName, string[] allowedExtensions)
        : base($"File '{fileName}' has invalid type. Allowed extensions: {string.Join(", ", allowedExtensions)}")
    {
        FileName = fileName;
        AllowedExtensions = allowedExtensions;
    }
}

public class MockFileProcessingException : MockFileUploadException
{
    public string FileName { get; }

    public MockFileProcessingException(string fileName, string message)
        : base($"Error processing file '{fileName}': {message}")
    {
        FileName = fileName;
    }
}