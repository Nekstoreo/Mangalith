using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mangalith.Api.Contracts;
using Mangalith.Application.Common.Exceptions;
using Mangalith.Application.Contracts.Files;
using Mangalith.Application.Interfaces.Services;

namespace Mangalith.Api.Controllers;

[ApiController]
[Route("api/files")]
[Authorize]
public class FilesController : ControllerBase
{
    private readonly IFileService _fileService;
    private readonly ILogger<FilesController> _logger;

    public FilesController(IFileService fileService, ILogger<FilesController> logger)
    {
        _fileService = fileService;
        _logger = logger;
    }

    [HttpPost("upload")]
    [ProducesResponseType(typeof(FileUploadResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status413PayloadTooLarge)]
    [RequestSizeLimit(100 * 1024 * 1024)] // 100MB limit
    public async Task<IActionResult> UploadFile([FromForm] FileUploadRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("File upload request from user {UserId} for file {FileName}", userId, request.File?.FileName);

            var response = await _fileService.UploadFileAsync(request, userId, cancellationToken);
            
            _logger.LogInformation("File upload successful: {FileId}", response.FileId);
            return CreatedAtAction(nameof(GetFile), new { id = response.FileId }, response);
        }
        catch (FileUploadException ex)
        {
            _logger.LogWarning(ex, "File upload failed: {Message}", ex.Message);
            return BadRequest(new ErrorResponse(
                "file_upload_error", 
                ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during file upload");
            return StatusCode(500, new ErrorResponse(
                "internal_error", 
                "An unexpected error occurred during file upload"));
        }
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetFile(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var fileStream = await _fileService.GetFileStreamAsync(id, cancellationToken);
            if (fileStream == null)
            {
                return NotFound(new ErrorResponse("file_not_found", "File not found"));
            }

            return File(fileStream, "application/octet-stream", enableRangeProcessing: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving file {FileId}", id);
            return StatusCode(500, new ErrorResponse(
                "internal_error", 
                "Error retrieving file"));
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteFile(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var success = await _fileService.DeleteFileAsync(id, userId, cancellationToken);
            
            if (!success)
            {
                return NotFound(new ErrorResponse("file_not_found", "File not found or access denied"));
            }

            _logger.LogInformation("File {FileId} deleted by user {UserId}", id, userId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {FileId}", id);
            return StatusCode(500, new ErrorResponse(
                "internal_error", 
                "Error deleting file"));
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user token");
        }
        return userId;
    }
}