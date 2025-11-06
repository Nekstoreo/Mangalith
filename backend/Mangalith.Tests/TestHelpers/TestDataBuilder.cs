using Mangalith.Domain.Entities;
using Mangalith.Domain.Enums;

namespace Mangalith.Tests.TestHelpers;

/// <summary>
/// Builder pattern para crear datos de prueba de manera consistente
/// </summary>
public static class TestDataBuilder
{
    public static UserBuilder CreateUser() => new();
    public static MangaBuilder CreateManga() => new();
    public static MangaFileBuilder CreateMangaFile() => new();
    public static PublicationBuilder CreatePublication() => new();
    public static ChapterBuilder CreateChapter() => new();
    public static UserQuotaBuilder CreateUserQuota() => new();
}

public class UserBuilder
{
    private string _email = "test@example.com";
    private string _passwordHash = "hashedPassword123";
    private string _fullName = "Test User";
    private string? _username = null;
    private UserRole _role = UserRole.Reader;

    public UserBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public UserBuilder WithPasswordHash(string passwordHash)
    {
        _passwordHash = passwordHash;
        return this;
    }

    public UserBuilder WithFullName(string fullName)
    {
        _fullName = fullName;
        return this;
    }

    public UserBuilder WithUsername(string username)
    {
        _username = username;
        return this;
    }

    public UserBuilder WithRole(UserRole role)
    {
        _role = role;
        return this;
    }

    public User Build()
    {
        var user = new User(_email, _passwordHash, _fullName, _username);
        if (_role != UserRole.Reader)
        {
            user.UpdateRole(_role);
        }
        return user;
    }
}

public class MangaBuilder
{
    private string _title = "Test Manga";
    private string? _description = "Test Description";
    private Guid _createdByUserId = Guid.NewGuid();
    private MangaStatus _status = MangaStatus.Draft;

    public MangaBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public MangaBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    public MangaBuilder WithCreatedByUserId(Guid userId)
    {
        _createdByUserId = userId;
        return this;
    }

    public MangaBuilder WithStatus(MangaStatus status)
    {
        _status = status;
        return this;
    }

    public Manga Build()
    {
        var manga = new Manga(_title, _description, _createdByUserId);
        if (_status != MangaStatus.Draft)
        {
            manga.UpdateStatus(_status);
        }
        return manga;
    }
}

public class MangaFileBuilder
{
    private Guid? _mangaId = Guid.NewGuid();
    private string _originalFileName = "test.cbz";
    private string _storedFileName = "stored_test.cbz";
    private string _filePath = "/test/path/stored_test.cbz";
    private long _fileSize = 1024L;
    private string _mimeType = "application/zip";
    private MangaFileType _fileType = MangaFileType.CBZ;
    private Guid _uploadedByUserId = Guid.NewGuid();
    private string? _fileHash = "ABC123DEF456";

    public MangaFileBuilder WithMangaId(Guid? mangaId)
    {
        _mangaId = mangaId;
        return this;
    }

    public MangaFileBuilder WithOriginalFileName(string originalFileName)
    {
        _originalFileName = originalFileName;
        return this;
    }

    public MangaFileBuilder WithStoredFileName(string storedFileName)
    {
        _storedFileName = storedFileName;
        return this;
    }

    public MangaFileBuilder WithFilePath(string filePath)
    {
        _filePath = filePath;
        return this;
    }

    public MangaFileBuilder WithFileSize(long fileSize)
    {
        _fileSize = fileSize;
        return this;
    }

    public MangaFileBuilder WithMimeType(string mimeType)
    {
        _mimeType = mimeType;
        return this;
    }

    public MangaFileBuilder WithFileType(MangaFileType fileType)
    {
        _fileType = fileType;
        return this;
    }

    public MangaFileBuilder WithUploadedByUserId(Guid uploadedByUserId)
    {
        _uploadedByUserId = uploadedByUserId;
        return this;
    }

    public MangaFileBuilder WithFileHash(string? fileHash)
    {
        _fileHash = fileHash;
        return this;
    }

    public MangaFileBuilder AsOrphanFile()
    {
        _mangaId = null;
        return this;
    }

    public MangaFile Build()
    {
        return new MangaFile(_mangaId, _originalFileName, _storedFileName, _filePath,
            _fileSize, _mimeType, _fileType, _uploadedByUserId, _fileHash);
    }
}

public class PublicationBuilder
{
    private Guid _mangaId = Guid.NewGuid();
    private Guid _createdByUserId = Guid.NewGuid();
    private PublicationStatus _status = PublicationStatus.Draft;
    private ContentRating _contentRating = ContentRating.General;
    private bool _isNsfw = false;

    public PublicationBuilder WithMangaId(Guid mangaId)
    {
        _mangaId = mangaId;
        return this;
    }

    public PublicationBuilder WithCreatedByUserId(Guid userId)
    {
        _createdByUserId = userId;
        return this;
    }

    public PublicationBuilder WithStatus(PublicationStatus status)
    {
        _status = status;
        return this;
    }

    public PublicationBuilder WithContentRating(ContentRating rating)
    {
        _contentRating = rating;
        return this;
    }

    public PublicationBuilder WithNsfw(bool isNsfw)
    {
        _isNsfw = isNsfw;
        return this;
    }

    public Publication Build()
    {
        var publication = new Publication(_mangaId, _createdByUserId);
        
        if (_status != PublicationStatus.Draft)
        {
            // Simulate state transitions
            switch (_status)
            {
                case PublicationStatus.InReview:
                    publication.SubmitForReview();
                    break;
                case PublicationStatus.Published:
                    publication.SubmitForReview();
                    publication.Approve(Guid.NewGuid(), _contentRating, _isNsfw);
                    break;
                case PublicationStatus.Rejected:
                    publication.SubmitForReview();
                    publication.Reject(Guid.NewGuid(), "Test rejection", "Test comments");
                    break;
                case PublicationStatus.NeedsRevision:
                    publication.SubmitForReview();
                    publication.RequestRevision(Guid.NewGuid(), "Test revision request");
                    break;
                case PublicationStatus.UnderReview:
                    publication.SubmitForReview();
                    publication.Approve(Guid.NewGuid(), _contentRating, _isNsfw);
                    publication.MarkUnderReview();
                    break;
                case PublicationStatus.Archived:
                    // Archived puede ser alcanzado desde Draft directamente
                    publication.Archive();
                    break;
            }
        }
        
        return publication;
    }
}

public class ChapterBuilder
{
    private Guid _mangaId = Guid.NewGuid();
    private string _title = "Test Chapter";
    private double _number = 1.0;
    private Guid _createdByUserId = Guid.NewGuid();
    private int? _volumeNumber = null;
    private ChapterStatus _status = ChapterStatus.Draft;
    private bool _isPublic = false;

    public ChapterBuilder WithMangaId(Guid mangaId)
    {
        _mangaId = mangaId;
        return this;
    }

    public ChapterBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public ChapterBuilder WithNumber(double number)
    {
        _number = number;
        return this;
    }

    public ChapterBuilder WithCreatedByUserId(Guid userId)
    {
        _createdByUserId = userId;
        return this;
    }

    public ChapterBuilder WithVolumeNumber(int volumeNumber)
    {
        _volumeNumber = volumeNumber;
        return this;
    }

    public ChapterBuilder WithStatus(ChapterStatus status)
    {
        _status = status;
        return this;
    }

    public ChapterBuilder WithPublic(bool isPublic)
    {
        _isPublic = isPublic;
        return this;
    }

    public Chapter Build()
    {
        var chapter = new Chapter(_mangaId, _title, _number, _createdByUserId);
        
        if (_volumeNumber.HasValue)
        {
            chapter.UpdateBasicInfo(_title, _number, _volumeNumber, null, null);
        }
        
        if (_status != ChapterStatus.Draft)
        {
            chapter.UpdateStatus(_status);
        }
        
        if (_isPublic)
        {
            chapter.SetPublic(_isPublic);
        }
        
        return chapter;
    }
}

public class UserQuotaBuilder
{
    private Guid _userId = Guid.NewGuid();
    private long _storageUsedBytes = 0;
    private int _filesUploadedToday = 0;
    private int _mangasCreated = 0;

    public UserQuotaBuilder WithUserId(Guid userId)
    {
        _userId = userId;
        return this;
    }

    public UserQuotaBuilder WithStorageUsed(long bytes)
    {
        _storageUsedBytes = bytes;
        return this;
    }

    public UserQuotaBuilder WithFilesUploadedToday(int count)
    {
        _filesUploadedToday = count;
        return this;
    }

    public UserQuotaBuilder WithMangasCreated(int count)
    {
        _mangasCreated = count;
        return this;
    }

    public UserQuota Build()
    {
        var quota = new UserQuota(_userId);
        
        if (_storageUsedBytes > 0)
        {
            quota.AddStorageUsage(_storageUsedBytes);
        }
        
        for (int i = 0; i < _filesUploadedToday; i++)
        {
            quota.IncrementFileUpload();
        }
        
        for (int i = 0; i < _mangasCreated; i++)
        {
            quota.IncrementMangaCreation();
        }
        
        return quota;
    }
}