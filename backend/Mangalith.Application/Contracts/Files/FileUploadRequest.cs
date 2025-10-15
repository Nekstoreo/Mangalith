using Microsoft.AspNetCore.Http;

namespace Mangalith.Application.Contracts.Files;

public class FileUploadRequest
{
    public IFormFile File { get; set; } = null!;
    public string? Title { get; set; }
    public string? Description { get; set; }
}