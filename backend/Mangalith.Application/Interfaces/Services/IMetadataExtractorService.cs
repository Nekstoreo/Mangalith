using Mangalith.Application.Services;

namespace Mangalith.Application.Interfaces.Services;

public interface IMetadataExtractorService
{
    ExtractedMetadata ExtractFromFilename(string filename);
    ExtractedMetadata ExtractFromDirectory(string directoryPath);
    bool ValidateMetadata(ExtractedMetadata metadata);
}
