namespace Mangalith.Application.Interfaces.Services;

public interface IMangaFileProcessorService
{
    Task<ProcessingResult> ProcessFileAsync(Guid fileId, CancellationToken cancellationToken = default);
}

public class ProcessingResult
{
    public bool Success { get; set; }
    public Guid? MangaId { get; set; }
    public Guid? ChapterId { get; set; }
    public int PageCount { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Warnings { get; set; } = new();
}
