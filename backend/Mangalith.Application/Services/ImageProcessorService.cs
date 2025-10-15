using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Mangalith.Application.Interfaces.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace Mangalith.Application.Services;

public class ImageProcessorService : IImageProcessorService
{
    private readonly ILogger<ImageProcessorService> _logger;

    public ImageProcessorService(ILogger<ImageProcessorService> logger)
    {
        _logger = logger;
    }

    public async Task<ImageInfo> GetImageInfoAsync(string imagePath, CancellationToken cancellationToken = default)
    {
        try
        {
            using var image = await Image.LoadAsync(imagePath, cancellationToken);
            var fileInfo = new FileInfo(imagePath);
            
            // Calculate hash
            using var stream = File.OpenRead(imagePath);
            using var sha256 = SHA256.Create();
            var hashBytes = await sha256.ComputeHashAsync(stream, cancellationToken);
            var hash = Convert.ToHexString(hashBytes);

            return new ImageInfo
            {
                Width = image.Width,
                Height = image.Height,
                FileSize = fileInfo.Length,
                Format = image.Metadata.DecodedImageFormat?.Name ?? "Unknown",
                Hash = hash
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting image info for {ImagePath}", imagePath);
            throw;
        }
    }

    public async Task OptimizeImageAsync(
        string sourcePath, 
        string destinationPath, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var image = await Image.LoadAsync(sourcePath, cancellationToken);
            
            // Optimize based on image size
            var maxDimension = Math.Max(image.Width, image.Height);
            
            // If image is too large, resize it
            if (maxDimension > 2000)
            {
                var scale = 2000.0 / maxDimension;
                var newWidth = (int)(image.Width * scale);
                var newHeight = (int)(image.Height * scale);
                
                image.Mutate(x => x.Resize(newWidth, newHeight));
                _logger.LogDebug("Resized image from {OrigWidth}x{OrigHeight} to {NewWidth}x{NewHeight}",
                    image.Width, image.Height, newWidth, newHeight);
            }

            // Save with optimized quality
            var encoder = new JpegEncoder
            {
                Quality = 85 // Good balance between quality and file size
            };

            await image.SaveAsync(destinationPath, encoder, cancellationToken);
            
            _logger.LogDebug("Optimized image saved to {DestinationPath}", destinationPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error optimizing image from {SourcePath} to {DestinationPath}", 
                sourcePath, destinationPath);
            
            // Fallback: just copy the file
            File.Copy(sourcePath, destinationPath, true);
        }
    }

    public async Task GenerateThumbnailAsync(
        string sourcePath, 
        string destinationPath, 
        int width, 
        int height, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var image = await Image.LoadAsync(sourcePath, cancellationToken);
            
            // Calculate aspect ratio
            var sourceAspect = (double)image.Width / image.Height;
            var targetAspect = (double)width / height;
            
            // Resize to cover the target dimensions
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(width, height),
                Mode = ResizeMode.Crop
            }));

            // Save as JPEG with good quality
            var encoder = new JpegEncoder { Quality = 90 };
            await image.SaveAsync(destinationPath, encoder, cancellationToken);
            
            _logger.LogDebug("Generated thumbnail {Width}x{Height} at {DestinationPath}", 
                width, height, destinationPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating thumbnail from {SourcePath}", sourcePath);
            throw;
        }
    }

    public async Task<bool> ValidateImageAsync(string imagePath, CancellationToken cancellationToken = default)
    {
        try
        {
            using var image = await Image.LoadAsync(imagePath, cancellationToken);
            
            // Basic validation
            if (image.Width < 100 || image.Height < 100)
            {
                _logger.LogWarning("Image {ImagePath} is too small: {Width}x{Height}", 
                    imagePath, image.Width, image.Height);
                return false;
            }

            if (image.Width > 10000 || image.Height > 10000)
            {
                _logger.LogWarning("Image {ImagePath} is too large: {Width}x{Height}", 
                    imagePath, image.Width, image.Height);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating image {ImagePath}", imagePath);
            return false;
        }
    }
}

public class ImageInfo
{
    public int Width { get; set; }
    public int Height { get; set; }
    public long FileSize { get; set; }
    public string Format { get; set; } = string.Empty;
    public string Hash { get; set; } = string.Empty;
}
