using Mangalith.Application.Services;

namespace Mangalith.Application.Interfaces.Services;

public interface IImageProcessorService
{
    Task<ImageInfo> GetImageInfoAsync(string imagePath, CancellationToken cancellationToken = default);
    Task OptimizeImageAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken = default);
    Task GenerateThumbnailAsync(string sourcePath, string destinationPath, int width, int height, CancellationToken cancellationToken = default);
    Task<bool> ValidateImageAsync(string imagePath, CancellationToken cancellationToken = default);
}
