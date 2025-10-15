using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mangalith.Application.Interfaces.Services;

namespace Mangalith.Application.Services;

public class BackgroundFileProcessorService : BackgroundService
{
    private readonly Channel<Guid> _processingQueue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BackgroundFileProcessorService> _logger;

    public BackgroundFileProcessorService(
        IServiceProvider serviceProvider,
        ILogger<BackgroundFileProcessorService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _processingQueue = Channel.CreateUnbounded<Guid>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });
    }

    public async Task QueueFileForProcessingAsync(Guid fileId)
    {
        await _processingQueue.Writer.WriteAsync(fileId);
        _logger.LogInformation("Queued file {FileId} for processing", fileId);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background file processor service started");

        await foreach (var fileId in _processingQueue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                _logger.LogInformation("Processing file {FileId}", fileId);

                using var scope = _serviceProvider.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<IMangaFileProcessorService>();
                
                var result = await processor.ProcessFileAsync(fileId, stoppingToken);
                
                if (result.Success)
                {
                    _logger.LogInformation(
                        "Successfully processed file {FileId}: Manga={MangaId}, Chapter={ChapterId}, Pages={PageCount}",
                        fileId, result.MangaId, result.ChapterId, result.PageCount);
                }
                else
                {
                    _logger.LogWarning("File processing completed with warnings: {Message}", result.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing file {FileId}", fileId);
            }
        }

        _logger.LogInformation("Background file processor service stopped");
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping background file processor service");
        _processingQueue.Writer.Complete();
        await base.StopAsync(cancellationToken);
    }
}
