namespace Mangalith.Application.Common.Configuration;

public class FileUploadOptions
{
    public const string SectionName = "FileUpload";
    
    public long MaxFileSizeBytes { get; set; } = 100 * 1024 * 1024; // 100MB default
    public string[] AllowedExtensions { get; set; } = { ".cbz", ".cbr", ".zip" };
    public string[] AllowedMimeTypes { get; set; } = { "application/zip", "application/x-cbz", "application/x-cbr" };
    public string UploadPath { get; set; } = "uploads";
    public string TempPath { get; set; } = "temp";
    public string ProcessingPath { get; set; } = "temp/processing";
    public string ChapterPagesPath { get; set; } = "data/chapters";
    public string ThumbnailsPath { get; set; } = "data/thumbnails";
}