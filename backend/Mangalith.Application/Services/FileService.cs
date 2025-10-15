using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mangalith.Application.Common.Configuration;
using Mangalith.Application.Common.Exceptions;
using Mangalith.Application.Contracts.Files;
using Mangalith.Application.Interfaces.Repositories;
using Mangalith.Application.Interfaces.Services;
using Mangalith.Domain.Entities;

namespace Mangalith.Application.Services;

public class FileService : IFileService
{
    private readonly IMangaFileRepository _mangaFileRepository;
    private readonly ILogger<FileService> _logger;
    private readonly FileUploadOptions _options;

    public FileService(
        IMangaFileRepository mangaFileRepository,
        ILogger<FileService> logger,
        IOptions<FileUploadOptions> options)
    {
        _mangaFileRepository = mangaFileRepository;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<FileUploadResponse> UploadFileAsync(FileUploadRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting file upload for user {UserId}, file: {FileName}", userId, request.File.FileName);

            // Validate file
            await ValidateFileAsync(request.File.OpenReadStream(), request.File.FileName, cancellationToken);

            // Generate file hash for duplicate detection
            var fileHash = await GenerateFileHashAsync(request.File.OpenReadStream(), cancellationToken);
            
            // Check for duplicates
            var existingFile = await _mangaFileRepository.GetByHashAsync(fileHash, cancellationToken);
            if (existingFile != null)
            {
                _logger.LogWarning("Duplicate file detected: {FileName} (Hash: {FileHash})", request.File.FileName, fileHash);
                throw new FileUploadException($"File '{request.File.FileName}' already exists in the system");
            }

            // Create directory structure
            var uploadPath = Path.Combine(_options.UploadPath, userId.ToString());
            Directory.CreateDirectory(uploadPath);

            // Generate unique file name
            var fileExtension = Path.GetExtension(request.File.FileName);
            var storedFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadPath, storedFileName);

            // Save file to disk
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await request.File.CopyToAsync(fileStream, cancellationToken);
            }

            // Create MangaFile entity without manga association (orphaned upload)
            // Will be properly linked when manga is created or during processing
            var mangaFile = new MangaFile(
                mangaId: null, // Orphaned file - no manga association yet
                originalFileName: request.File.FileName,
                storedFileName: storedFileName,
                filePath: filePath,
                fileSize: request.File.Length,
                mimeType: request.File.ContentType,
                fileType: GetFileTypeFromExtension(fileExtension),
                uploadedByUserId: userId,
                fileHash: fileHash
            );

            // Save to database
            await _mangaFileRepository.AddAsync(mangaFile, cancellationToken);

            _logger.LogInformation("File uploaded successfully: {FileId} for user {UserId}", mangaFile.Id, userId);

            return new FileUploadResponse
            {
                FileId = mangaFile.Id,
                OriginalFileName = mangaFile.OriginalFileName,
                FileSize = mangaFile.FileSize,
                FileType = mangaFile.FileType,
                Status = mangaFile.Status,
                UploadedAt = mangaFile.CreatedAtUtc,
                Message = "File uploaded successfully"
            };
        }
        catch (Exception ex) when (!(ex is FileUploadException))
        {
            _logger.LogError(ex, "Unexpected error during file upload for user {UserId}", userId);
            throw new FileUploadException("An unexpected error occurred during file upload", ex);
        }
    }

    public async Task<bool> DeleteFileAsync(Guid fileId, Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var mangaFile = await _mangaFileRepository.GetByIdAsync(fileId, cancellationToken);
            if (mangaFile == null)
            {
                _logger.LogWarning("File not found for deletion: {FileId}", fileId);
                return false;
            }

            // Check if user owns the file
            if (mangaFile.UploadedByUserId != userId)
            {
                _logger.LogWarning("User {UserId} attempted to delete file {FileId} they don't own", userId, fileId);
                return false;
            }

            // Delete physical file
            if (File.Exists(mangaFile.FilePath))
            {
                File.Delete(mangaFile.FilePath);
            }

            // Delete from database
            await _mangaFileRepository.DeleteAsync(fileId, cancellationToken);

            _logger.LogInformation("File deleted successfully: {FileId} by user {UserId}", fileId, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {FileId} for user {UserId}", fileId, userId);
            return false;
        }
    }

    public async Task<Stream?> GetFileStreamAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        var mangaFile = await _mangaFileRepository.GetByIdAsync(fileId, cancellationToken);
        if (mangaFile == null || !File.Exists(mangaFile.FilePath))
        {
            return null;
        }

        return new FileStream(mangaFile.FilePath, FileMode.Open, FileAccess.Read);
    }

    public async Task<bool> ValidateFileAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        // Check file size
        if (fileStream.Length > _options.MaxFileSizeBytes)
        {
            throw new FileSizeExceededException(fileName, fileStream.Length, _options.MaxFileSizeBytes);
        }

        // Check file extension
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!_options.AllowedExtensions.Contains(extension))
        {
            throw new InvalidFileTypeException(fileName, _options.AllowedExtensions);
        }

        // Basic file validation - check if it's a valid archive
        try
        {
            fileStream.Position = 0;
            var buffer = new byte[4];
            await fileStream.ReadExactlyAsync(buffer, 0, 4, cancellationToken);
            fileStream.Position = 0;

            // Check for ZIP signature (PK)
            if (buffer[0] == 0x50 && buffer[1] == 0x4B)
            {
                return true;
            }

            // Check for RAR signature
            if (buffer[0] == 0x52 && buffer[1] == 0x61 && buffer[2] == 0x72 && buffer[3] == 0x21)
            {
                return true;
            }

            throw new FileProcessingException(fileName, "File does not appear to be a valid archive");
        }
        catch (Exception ex) when (!(ex is FileUploadException))
        {
            throw new FileProcessingException(fileName, $"Error validating file: {ex.Message}");
        }
    }

    private static async Task<string> GenerateFileHashAsync(Stream fileStream, CancellationToken cancellationToken)
    {
        fileStream.Position = 0;
        using var sha256 = SHA256.Create();
        var hashBytes = await sha256.ComputeHashAsync(fileStream, cancellationToken);
        fileStream.Position = 0;
        return Convert.ToHexString(hashBytes);
    }

    private static MangaFileType GetFileTypeFromExtension(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".cbz" => MangaFileType.CBZ,
            ".cbr" => MangaFileType.CBR,
            ".zip" => MangaFileType.ZIP,
            ".rar" => MangaFileType.RAR,
            ".pdf" => MangaFileType.PDF,
            _ => MangaFileType.Unknown
        };
    }
}