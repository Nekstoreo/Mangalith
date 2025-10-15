namespace Mangalith.Domain.Entities;

public class ChapterPage
{
    public Guid Id { get; private set; }
    public Guid ChapterId { get; private set; }
    public int PageNumber { get; private set; }
    public string ImagePath { get; private set; }
    public string? ImageHash { get; private set; } // For duplicate detection
    public int Width { get; private set; }
    public int Height { get; private set; }
    public long FileSize { get; private set; }
    public string MimeType { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    // Navigation properties
    public Chapter Chapter { get; private set; } = null!;

    private ChapterPage()
    {
        Id = Guid.Empty;
        ChapterId = Guid.Empty;
        ImagePath = string.Empty;
        MimeType = string.Empty;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public ChapterPage(Guid chapterId, int pageNumber, string imagePath, string mimeType, 
        int width, int height, long fileSize, string? imageHash = null)
    {
        Id = Guid.NewGuid();
        ChapterId = chapterId;
        PageNumber = pageNumber;
        ImagePath = imagePath;
        MimeType = mimeType;
        Width = width;
        Height = height;
        FileSize = fileSize;
        ImageHash = imageHash;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateImageInfo(int width, int height, long fileSize, string? imageHash = null)
    {
        Width = width;
        Height = height;
        FileSize = fileSize;
        ImageHash = imageHash;
    }

    public void UpdatePath(string newImagePath)
    {
        ImagePath = newImagePath;
    }
}