namespace Mangalith.Domain.Entities;

public class MangaFile
{
    public Guid Id { get; private set; }
    public Guid? MangaId { get; private set; } // Nullable para permitir archivos huérfanos durante la carga
    public string OriginalFileName { get; private set; }
    public string StoredFileName { get; private set; }
    public string FilePath { get; private set; }
    public long FileSize { get; private set; }
    public string MimeType { get; private set; }
    public string? FileHash { get; private set; } // Para detección de duplicados
    public MangaFileType FileType { get; private set; }
    public MangaFileStatus Status { get; private set; }
    public string? ProcessingError { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public Guid UploadedByUserId { get; private set; }

    // Propiedades de navegación
    public Manga? Manga { get; private set; } // Navegación nullable
    public User UploadedByUser { get; private set; } = null!;

    private MangaFile()
    {
        Id = Guid.Empty;
        MangaId = null; // Cambiado a null
        OriginalFileName = string.Empty;
        StoredFileName = string.Empty;
        FilePath = string.Empty;
        MimeType = string.Empty;
        FileType = MangaFileType.Unknown;
        Status = MangaFileStatus.Uploaded;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
        UploadedByUserId = Guid.Empty;
    }

    public MangaFile(Guid? mangaId, string originalFileName, string storedFileName, 
        string filePath, long fileSize, string mimeType, MangaFileType fileType, 
        Guid uploadedByUserId, string? fileHash = null)
    {
        Id = Guid.NewGuid();
        MangaId = mangaId; // Puede ser null para archivos huérfanos
        OriginalFileName = originalFileName;
        StoredFileName = storedFileName;
        FilePath = filePath;
        FileSize = fileSize;
        MimeType = mimeType;
        FileType = fileType;
        Status = MangaFileStatus.Uploaded;
        UploadedByUserId = uploadedByUserId;
        FileHash = fileHash;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateStatus(MangaFileStatus status, string? processingError = null)
    {
        Status = status;
        ProcessingError = processingError;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateFileInfo(long fileSize, string? fileHash = null)
    {
        FileSize = fileSize;
        FileHash = fileHash;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}

public enum MangaFileType
{
    Unknown = 0,
    CBZ = 1,
    CBR = 2,
    ZIP = 3,
    RAR = 4,
    PDF = 5
}

public enum MangaFileStatus
{
    Uploaded = 0,
    Processing = 1,
    Processed = 2,
    Failed = 3,
    Archived = 4
}