using Mangalith.Domain.Entities;

namespace Mangalith.Application.Contracts.Files;

public class FileUploadResponse
{
    public Guid FileId { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public MangaFileType FileType { get; set; }
    public MangaFileStatus Status { get; set; }
    public DateTime UploadedAt { get; set; }
    public string Message { get; set; } = string.Empty;
}