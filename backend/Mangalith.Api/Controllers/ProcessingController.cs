using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mangalith.Application.Interfaces.Services;
using Mangalith.Application.Services;

namespace Mangalith.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProcessingController : ControllerBase
{
    private readonly IMangaFileProcessorService _processor;
    private readonly BackgroundFileProcessorService _backgroundProcessor;
    private readonly ILogger<ProcessingController> _logger;

    public ProcessingController(
        IMangaFileProcessorService processor,
        BackgroundFileProcessorService backgroundProcessor,
        ILogger<ProcessingController> logger)
    {
        _processor = processor;
        _backgroundProcessor = backgroundProcessor;
        _logger = logger;
    }

    [HttpPost("{fileId}/process")]
    public async Task<IActionResult> ProcessFile(Guid fileId, [FromQuery] bool async = true)
    {
        try
        {
            if (async)
            {
                // Encolar para procesamiento en segundo plano
                await _backgroundProcessor.QueueFileForProcessingAsync(fileId);
                return Accepted(new { message = "File queued for processing", fileId });
            }
            else
            {
                // Procesar sincrónicamente
                var result = await _processor.ProcessFileAsync(fileId);
                return Ok(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing file {FileId}", fileId);
            return StatusCode(500, new { error = "Error processing file", message = ex.Message });
        }
    }

    [HttpPost("batch")]
    public async Task<IActionResult> ProcessBatch([FromBody] List<Guid> fileIds)
    {
        try
        {
            foreach (var fileId in fileIds)
            {
                await _backgroundProcessor.QueueFileForProcessingAsync(fileId);
            }

            return Accepted(new { message = $"Queued {fileIds.Count} files for processing", count = fileIds.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error queuing batch processing");
            return StatusCode(500, new { error = "Error queuing files", message = ex.Message });
        }
    }
}
