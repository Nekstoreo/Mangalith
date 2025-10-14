using Mangalith.Application.Contracts.Files;

namespace Mangalith.Application.Interfaces.Services;

public interface IFileService
{
    Task<FileUploadResponse> UploadFileAsync(FileUploadRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteFileAsync(Guid fileId, Guid userId, CancellationToken cancellationToken = default);
    Task<Stream?> GetFileStreamAsync(Guid fileId, CancellationToken cancellationToken = default);
    Task<bool> ValidateFileAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);
}