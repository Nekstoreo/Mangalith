using System.IO.Compression;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mangalith.Application.Common.Configuration;
using Mangalith.Application.Common.Exceptions;
using Mangalith.Application.Interfaces.Repositories;
using Mangalith.Application.Interfaces.Services;
using Mangalith.Domain.Entities;

namespace Mangalith.Application.Services;

public class MangaFileProcessorService : IMangaFileProcessorService
{
    private readonly IMangaFileRepository _mangaFileRepository;
    private readonly IMangaRepository _mangaRepository;
    private readonly IChapterRepository _chapterRepository;
    private readonly IImageProcessorService _imageProcessor;
    private readonly IMetadataExtractorService _metadataExtractor;
    private readonly ILogger<MangaFileProcessorService> _logger;
    private readonly FileUploadOptions _options;

    private static readonly string[] SupportedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".bmp" };

    public MangaFileProcessorService(
        IMangaFileRepository mangaFileRepository,
        IMangaRepository mangaRepository,
        IChapterRepository chapterRepository,
        IImageProcessorService imageProcessor,
        IMetadataExtractorService metadataExtractor,
        ILogger<MangaFileProcessorService> logger,
        IOptions<FileUploadOptions> options)
    {
        _mangaFileRepository = mangaFileRepository;
        _mangaRepository = mangaRepository;
        _chapterRepository = chapterRepository;
        _imageProcessor = imageProcessor;
        _metadataExtractor = metadataExtractor;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<ProcessingResult> ProcessFileAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        var mangaFile = await _mangaFileRepository.GetByIdAsync(fileId, cancellationToken);
        if (mangaFile == null)
        {
            throw new NotFoundException($"MangaFile with ID {fileId} not found");
        }

        try
        {
            _logger.LogInformation("Starting processing for file {FileId}: {FileName}", fileId, mangaFile.OriginalFileName);
            
            // Actualizar estado a procesando
            mangaFile.UpdateStatus(MangaFileStatus.Processing);
            await _mangaFileRepository.UpdateAsync(mangaFile, cancellationToken);

            // Extraer metadatos del nombre de archivo
            var metadata = _metadataExtractor.ExtractFromFilename(mangaFile.OriginalFileName);
            
            // Procesar según el tipo de archivo
            var result = mangaFile.FileType switch
            {
                MangaFileType.CBZ or MangaFileType.ZIP => await ProcessZipArchiveAsync(mangaFile, metadata, cancellationToken),
                MangaFileType.CBR or MangaFileType.RAR => await ProcessRarArchiveAsync(mangaFile, metadata, cancellationToken),
                _ => throw new FileProcessingException(mangaFile.OriginalFileName, "Unsupported file type")
            };

            // Actualizar estado a procesado
            mangaFile.UpdateStatus(MangaFileStatus.Processed);
            await _mangaFileRepository.UpdateAsync(mangaFile, cancellationToken);

            _logger.LogInformation("Successfully processed file {FileId}", fileId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing file {FileId}: {FileName}", fileId, mangaFile.OriginalFileName);
            mangaFile.UpdateStatus(MangaFileStatus.Failed, ex.Message);
            await _mangaFileRepository.UpdateAsync(mangaFile, cancellationToken);
            throw;
        }
    }

    private async Task<ProcessingResult> ProcessZipArchiveAsync(
        MangaFile mangaFile, 
        ExtractedMetadata metadata, 
        CancellationToken cancellationToken)
    {
        var extractPath = Path.Combine(_options.ProcessingPath, mangaFile.Id.ToString());
        Directory.CreateDirectory(extractPath);

        try
        {
            // Extraer archivo
            using (var archive = ZipFile.OpenRead(mangaFile.FilePath))
            {
                var imageEntries = archive.Entries
                    .Where(e => !string.IsNullOrEmpty(e.Name) && 
                               SupportedImageExtensions.Contains(Path.GetExtension(e.Name).ToLowerInvariant()))
                    .OrderBy(e => e.FullName, new NaturalStringComparer())
                    .ToList();

                if (imageEntries.Count == 0)
                {
                    throw new FileProcessingException(mangaFile.OriginalFileName, "No valid images found in archive");
                }

                _logger.LogInformation("Found {Count} images in archive {FileId}", imageEntries.Count, mangaFile.Id);

                // Crear u obtener manga
                var manga = await GetOrCreateMangaAsync(mangaFile, metadata, cancellationToken);
                
                // Crear capítulo
                var chapter = await CreateChapterAsync(manga.Id, metadata, mangaFile.UploadedByUserId, cancellationToken);

                // Procesar imágenes
                var pages = new List<ChapterPage>();
                int pageNumber = 1;

                foreach (var entry in imageEntries)
                {
                    var page = await ProcessImageEntryAsync(
                        entry, 
                        chapter.Id, 
                        pageNumber++, 
                        extractPath, 
                        cancellationToken);
                    
                    if (page != null)
                    {
                        pages.Add(page);
                    }
                }

                // Actualizar conteo de páginas del capítulo
                chapter.UpdatePageCount(pages.Count);
                await _chapterRepository.UpdateAsync(chapter, cancellationToken);

                // Generar miniatura de portada si existe la primera página
                if (pages.Count > 0)
                {
                    await GenerateCoverThumbnailAsync(manga, pages[0].ImagePath, cancellationToken);
                }

                return new ProcessingResult
                {
                    Success = true,
                    MangaId = manga.Id,
                    ChapterId = chapter.Id,
                    PageCount = pages.Count,
                    Message = "File processed successfully"
                };
            }
        }
        finally
        {
            // Limpiar directorio de extracción
            if (Directory.Exists(extractPath))
            {
                Directory.Delete(extractPath, true);
            }
        }
    }

    private Task<ProcessingResult> ProcessRarArchiveAsync(
        MangaFile mangaFile, 
        ExtractedMetadata metadata, 
        CancellationToken cancellationToken)
    {
        // El procesamiento RAR requiere biblioteca externa (SharpCompress)
        // Por ahora, lanzar no implementado - se agregará con el paquete SharpCompress
        throw new FileProcessingException(
            mangaFile.OriginalFileName, 
            "RAR file processing requires additional setup. Please convert to ZIP/CBZ format.");
    }

    private async Task<ChapterPage?> ProcessImageEntryAsync(
        ZipArchiveEntry entry,
        Guid chapterId,
        int pageNumber,
        string extractPath,
        CancellationToken cancellationToken)
    {
        try
        {
            var tempImagePath = Path.Combine(extractPath, $"page_{pageNumber}{Path.GetExtension(entry.Name)}");
            
            // Extraer imagen
            entry.ExtractToFile(tempImagePath, true);

            // Obtener dimensiones de imagen y optimizar
            var imageInfo = await _imageProcessor.GetImageInfoAsync(tempImagePath, cancellationToken);
            
            // Crear ruta de almacenamiento permanente
            var permanentPath = Path.Combine(
                _options.ChapterPagesPath, 
                chapterId.ToString(), 
                $"{pageNumber:D4}{Path.GetExtension(entry.Name)}");
            
            Directory.CreateDirectory(Path.GetDirectoryName(permanentPath)!);

            // Optimizar y guardar imagen
            await _imageProcessor.OptimizeImageAsync(tempImagePath, permanentPath, cancellationToken);

            // Crear entidad ChapterPage
            var page = new ChapterPage(
                chapterId: chapterId,
                pageNumber: pageNumber,
                imagePath: permanentPath,
                mimeType: GetMimeType(Path.GetExtension(entry.Name)),
                width: imageInfo.Width,
                height: imageInfo.Height,
                fileSize: new FileInfo(permanentPath).Length,
                imageHash: imageInfo.Hash
            );

            return page;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing image entry {EntryName} for chapter {ChapterId}", 
                entry.FullName, chapterId);
            return null;
        }
    }

    private async Task<Manga> GetOrCreateMangaAsync(
        MangaFile mangaFile, 
        ExtractedMetadata metadata, 
        CancellationToken cancellationToken)
    {
        // Si el archivo ya tiene asociación de manga, devolverlo
        if (mangaFile.MangaId.HasValue)
        {
            var existingManga = await _mangaRepository.GetByIdAsync(mangaFile.MangaId.Value, cancellationToken);
            if (existingManga != null)
            {
                return existingManga;
            }
        }

        // Crear nuevo manga desde metadatos
        var manga = new Manga(
            title: metadata.Title ?? Path.GetFileNameWithoutExtension(mangaFile.OriginalFileName),
            description: null,
            createdByUserId: mangaFile.UploadedByUserId
        );

        if (!string.IsNullOrEmpty(metadata.Author))
        {
            manga.UpdateBasicInfo(
                manga.Title,
                null,
                null,
                metadata.Author,
                null,
                metadata.Year
            );
        }

        await _mangaRepository.AddAsync(manga, cancellationToken);
        
        _logger.LogInformation("Created new manga {MangaId}: {Title}", manga.Id, manga.Title);
        
        return manga;
    }

    private async Task<Chapter> CreateChapterAsync(
        Guid mangaId, 
        ExtractedMetadata metadata, 
        Guid userId, 
        CancellationToken cancellationToken)
    {
        var chapterNumber = metadata.ChapterNumber ?? 1.0;
        var chapterTitle = metadata.ChapterTitle ?? $"Chapter {chapterNumber}";

        var chapter = new Chapter(
            mangaId: mangaId,
            title: chapterTitle,
            number: chapterNumber,
            createdByUserId: userId
        );

        if (metadata.VolumeNumber.HasValue)
        {
            chapter.UpdateBasicInfo(
                chapterTitle,
                chapterNumber,
                metadata.VolumeNumber,
                null,
                null
            );
        }

        await _chapterRepository.AddAsync(chapter, cancellationToken);
        
        _logger.LogInformation("Created chapter {ChapterId}: {Title} for manga {MangaId}", 
            chapter.Id, chapterTitle, mangaId);
        
        return chapter;
    }

    private async Task GenerateCoverThumbnailAsync(
        Manga manga, 
        string firstPagePath, 
        CancellationToken cancellationToken)
    {
        try
        {
            var thumbnailPath = Path.Combine(
                _options.ThumbnailsPath, 
                "covers",
                $"{manga.Id}.jpg"
            );

            Directory.CreateDirectory(Path.GetDirectoryName(thumbnailPath)!);

            await _imageProcessor.GenerateThumbnailAsync(
                firstPagePath, 
                thumbnailPath, 
                300, 
                450, 
                cancellationToken);

            manga.UpdateCoverImage(thumbnailPath);
            await _mangaRepository.UpdateAsync(manga, cancellationToken);

            _logger.LogInformation("Generated cover thumbnail for manga {MangaId}", manga.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating cover thumbnail for manga {MangaId}", manga.Id);
        }
    }

    private static string GetMimeType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            _ => "application/octet-stream"
        };
    }
}

// Comparador de cadenas natural para ordenamiento apropiado de archivos (1, 2, 10 en lugar de 1, 10, 2)
public class NaturalStringComparer : IComparer<string>
{
    public int Compare(string? x, string? y)
    {
        if (x == null && y == null) return 0;
        if (x == null) return -1;
        if (y == null) return 1;

        var regex = new Regex(@"(\d+)|(\D+)");
        var xParts = regex.Matches(x).Cast<Match>().Select(m => m.Value).ToArray();
        var yParts = regex.Matches(y).Cast<Match>().Select(m => m.Value).ToArray();

        for (int i = 0; i < Math.Min(xParts.Length, yParts.Length); i++)
        {
            if (int.TryParse(xParts[i], out var xNum) && int.TryParse(yParts[i], out var yNum))
            {
                var numCompare = xNum.CompareTo(yNum);
                if (numCompare != 0) return numCompare;
            }
            else
            {
                var strCompare = string.Compare(xParts[i], yParts[i], StringComparison.OrdinalIgnoreCase);
                if (strCompare != 0) return strCompare;
            }
        }

        return xParts.Length.CompareTo(yParts.Length);
    }
}
