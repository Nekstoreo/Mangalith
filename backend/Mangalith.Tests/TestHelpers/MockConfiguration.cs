namespace Mangalith.Tests.TestHelpers;

public class MockFileUploadOptions
{
    public string UploadPath { get; set; } = string.Empty;
    public long MaxFileSizeBytes { get; set; }
    public string[] AllowedExtensions { get; set; } = Array.Empty<string>();
}