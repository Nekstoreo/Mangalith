using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mangalith.Application.Common.Configuration;
using Mangalith.Application.Common.Exceptions;
using Mangalith.Application.Contracts.Files;
using Mangalith.Application.Interfaces.Repositories;
using Mangalith.Application.Interfaces.Services;
using Mangalith.Domain.Constants;
using Mangalith.Domain.Entities;

namespace Mangalith.Application.Services;

public class FileService : IFileService
{
    private readonly IMangaFileRepository _mangaFileRepository;
    private readonly IUserRepository _userRepository;
    private readonly IQuotaService _quotaService;
    private readonly ILogger<FileService> _logger;
    private readonly FileUploadOptions _options;

    public FileService(
        IMangaFileRepository mangaFileRepository,
        IUserRepository userRepository,
        IQuotaService quotaService,
        ILogger<FileService> logger,
        IOptions<FileUploadOptions> options)
    {
        _mangaFileRepository = mangaFileRepository;
        _userRepository = userRepository;
        _quotaService = quotaService;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<FileUploadResponse> UploadFileAsync(FileUploadRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting file upload for user {UserId}, file: {FileName}", userId, request.File.FileName);

            // Verificar cuotas antes de procesar el archivo
            var canUpload = await _quotaService.CanUploadFileAsync(userId, request.File.Length, cancellationToken);
            if (!canUpload)
            {
                var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
                var quotaReport = await _quotaService.GetQuotaUsageReportAsync(userId, cancellationToken);
                
                var errorMessage = "Upload failed due to quota restrictions.";
                if (quotaReport.HasExceededAnyLimit)
                {
                    if (quotaReport.StorageUsagePercentage >= 100)
                    {
                        errorMessage = $"Storage quota exceeded. You are using {FormatBytes(quotaReport.StorageUsedBytes)} of {FormatBytes(quotaReport.StorageQuotaBytes)} available.";
                    }
                    else if (quotaReport.FilesUploadedToday >= quotaReport.DailyUploadLimit)
                    {
                        errorMessage = $"Daily upload limit exceeded. You have uploaded {quotaReport.FilesUploadedToday} of {quotaReport.DailyUploadLimit} files today.";
                    }
                }
                else if (request.File.Length > QuotaLimits.GetMaxFileSize(user?.Role ?? UserRole.Reader))
                {
                    var maxSize = QuotaLimits.GetMaxFileSize(user?.Role ?? UserRole.Reader);
                    errorMessage = $"File size {FormatBytes(request.File.Length)} exceeds maximum allowed size of {FormatBytes(maxSize)} for your role.";
                }

                _logger.LogWarning("File upload blocked for user {UserId}: {Reason}", userId, errorMessage);
                throw new QuotaExceededException("file_upload", request.File.Length, quotaReport.StorageQuotaBytes);
            }

            // Validar archivo
            await ValidateFileAsync(request.File.OpenReadStream(), request.File.FileName, cancellationToken);

            // Generar hash de archivo para detección de duplicados
            var fileHash = await GenerateFileHashAsync(request.File.OpenReadStream(), cancellationToken);
            
            // Verificar duplicados
            var existingFile = await _mangaFileRepository.GetByHashAsync(fileHash, cancellationToken);
            if (existingFile != null)
            {
                _logger.LogWarning("Duplicate file detected: {FileName} (Hash: {FileHash})", request.File.FileName, fileHash);
                throw new FileUploadException($"File '{request.File.FileName}' already exists in the system");
            }

            // Crear estructura de directorios
            var uploadPath = Path.Combine(_options.UploadPath, userId.ToString());
            Directory.CreateDirectory(uploadPath);

            // Generar nombre de archivo único
            var fileExtension = Path.GetExtension(request.File.FileName);
            var storedFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadPath, storedFileName);

            // Guardar archivo en disco
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await request.File.CopyToAsync(fileStream, cancellationToken);
            }

            // Crear entidad MangaFile sin asociación de manga (carga huérfana)
            // Se vinculará apropiadamente cuando se cree el manga o durante el procesamiento
            var mangaFile = new MangaFile(
                mangaId: null, // Archivo huérfano - sin asociación de manga aún
                originalFileName: request.File.FileName,
                storedFileName: storedFileName,
                filePath: filePath,
                fileSize: request.File.Length,
                mimeType: request.File.ContentType,
                fileType: GetFileTypeFromExtension(fileExtension),
                uploadedByUserId: userId,
                fileHash: fileHash
            );

            // Guardar en base de datos
            await _mangaFileRepository.AddAsync(mangaFile, cancellationToken);

            // Actualizar cuotas del usuario
            await _quotaService.TrackFileUploadAsync(userId, request.File.Length, cancellationToken);

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

            // Verificar si el usuario es propietario del archivo
            if (mangaFile.UploadedByUserId != userId)
            {
                _logger.LogWarning("User {UserId} attempted to delete file {FileId} they don't own", userId, fileId);
                return false;
            }

            // Eliminar archivo físico
            if (File.Exists(mangaFile.FilePath))
            {
                File.Delete(mangaFile.FilePath);
            }

            // Eliminar de la base de datos
            await _mangaFileRepository.DeleteAsync(fileId, cancellationToken);

            // Actualizar cuotas del usuario
            await _quotaService.TrackFileDeleteAsync(userId, mangaFile.FileSize, cancellationToken);

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
        // Verificar tamaño de archivo
        if (fileStream.Length > _options.MaxFileSizeBytes)
        {
            throw new FileSizeExceededException(fileName, fileStream.Length, _options.MaxFileSizeBytes);
        }

        // Verificar extensión de archivo
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!_options.AllowedExtensions.Contains(extension))
        {
            throw new InvalidFileTypeException(fileName, _options.AllowedExtensions);
        }

        // Validación básica de archivo - verificar si es un archivo válido
        try
        {
            fileStream.Position = 0;
            var buffer = new byte[4];
            await fileStream.ReadExactlyAsync(buffer, 0, 4, cancellationToken);
            fileStream.Position = 0;

            // Verificar firma ZIP (PK)
            if (buffer[0] == 0x50 && buffer[1] == 0x4B)
            {
                return true;
            }

            // Verificar firma RAR
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

    private static string FormatBytes(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024.0 * 1024):F1} MB";
        return $"{bytes / (1024.0 * 1024 * 1024):F1} GB";
    }
}