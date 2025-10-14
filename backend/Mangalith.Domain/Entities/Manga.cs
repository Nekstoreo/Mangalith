namespace Mangalith.Domain.Entities;

public class Manga
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public string? AlternativeTitle { get; private set; }
    public string? Description { get; private set; }
    public string? Author { get; private set; }
    public string? Artist { get; private set; }
    public int? Year { get; private set; }
    public MangaStatus Status { get; private set; }
    public string? CoverImagePath { get; private set; }
    public string? Tags { get; private set; } // JSON array of tags
    public string? Genres { get; private set; } // JSON array of genres
    public int ChapterCount { get; private set; }
    public int ViewCount { get; private set; }
    public double Rating { get; private set; }
    public int RatingCount { get; private set; }
    public bool IsPublic { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public Guid CreatedByUserId { get; private set; }

    // Navigation properties
    public User CreatedByUser { get; private set; } = null!;
    public ICollection<Chapter> Chapters { get; private set; } = new List<Chapter>();
    public ICollection<MangaFile> Files { get; private set; } = new List<MangaFile>();

    private Manga()
    {
        Id = Guid.Empty;
        Title = string.Empty;
        Status = MangaStatus.Unknown;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
        CreatedByUserId = Guid.Empty;
    }

    public Manga(string title, Guid createdByUserId)
    {
        Id = Guid.NewGuid();
        Title = title;
        Status = MangaStatus.Ongoing;
        IsPublic = false;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
        CreatedByUserId = createdByUserId;
    }

    public void UpdateBasicInfo(string title, string? alternativeTitle, string? description, 
        string? author, string? artist, int? year)
    {
        Title = title;
        AlternativeTitle = alternativeTitle;
        Description = description;
        Author = author;
        Artist = artist;
        Year = year;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateStatus(MangaStatus status)
    {
        Status = status;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateCoverImage(string coverImagePath)
    {
        CoverImagePath = coverImagePath;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateTags(string tags)
    {
        Tags = tags;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateGenres(string genres)
    {
        Genres = genres;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void IncrementViewCount()
    {
        ViewCount++;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateRating(double newRating, int newRatingCount)
    {
        Rating = newRating;
        RatingCount = newRatingCount;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void SetPublic(bool isPublic)
    {
        IsPublic = isPublic;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateChapterCount(int count)
    {
        ChapterCount = count;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}

public enum MangaStatus
{
    Unknown = 0,
    Ongoing = 1,
    Completed = 2,
    Hiatus = 3,
    Cancelled = 4,
    Draft = 5
}