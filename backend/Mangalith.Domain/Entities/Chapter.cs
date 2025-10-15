namespace Mangalith.Domain.Entities;

public class Chapter
{
    public Guid Id { get; private set; }
    public Guid MangaId { get; private set; }
    public string Title { get; private set; }
    public double Number { get; private set; } // Permite 1.5, 2.1, etc.
    public int? VolumeNumber { get; private set; }
    public int PageCount { get; private set; }
    public string? Notes { get; private set; }
    public string? TranslatorNotes { get; private set; }
    public ChapterStatus Status { get; private set; }
    public bool IsPublic { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public DateTime? PublishedAtUtc { get; private set; }
    public Guid CreatedByUserId { get; private set; }

    // Propiedades de navegación
    public Manga Manga { get; private set; } = null!;
    public User CreatedByUser { get; private set; } = null!;
    public ICollection<ChapterPage> Pages { get; private set; } = new List<ChapterPage>();

    private Chapter()
    {
        Id = Guid.Empty;
        MangaId = Guid.Empty;
        Title = string.Empty;
        Status = ChapterStatus.Draft;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
        CreatedByUserId = Guid.Empty;
    }

    public Chapter(Guid mangaId, string title, double number, Guid createdByUserId)
    {
        Id = Guid.NewGuid();
        MangaId = mangaId;
        Title = title;
        Number = number;
        Status = ChapterStatus.Draft;
        IsPublic = false;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
        CreatedByUserId = createdByUserId;
    }

    public void UpdateBasicInfo(string title, double number, int? volumeNumber, string? notes, string? translatorNotes)
    {
        Title = title;
        Number = number;
        VolumeNumber = volumeNumber;
        Notes = notes;
        TranslatorNotes = translatorNotes;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateStatus(ChapterStatus status)
    {
        Status = status;
        if (status == ChapterStatus.Published && !PublishedAtUtc.HasValue)
        {
            PublishedAtUtc = DateTime.UtcNow;
        }
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void SetPublic(bool isPublic)
    {
        IsPublic = isPublic;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdatePageCount(int count)
    {
        PageCount = count;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}

public enum ChapterStatus
{
    Draft = 0,
    Processing = 1,
    Ready = 2,
    Published = 3,
    Archived = 4
}