using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Mangalith.Application.Interfaces.Services;

namespace Mangalith.Application.Services;

public class MetadataExtractorService : IMetadataExtractorService
{
    private readonly ILogger<MetadataExtractorService> _logger;

    // Patrones comunes de nombres de archivos de manga
    private static readonly Regex[] FilenamePatterns = 
    {
        // [Grupo] Título - Capítulo X [Extra].ext
        new Regex(@"^\[(?<group>[^\]]+)\]\s*(?<title>.+?)\s*-\s*(?:Chapter|Ch\.?|c)\s*(?<chapter>[\d.]+)(?:\s*-\s*(?<chapterTitle>[^\[]+))?(?:\s*\[(?<extra>[^\]]+)\])?", RegexOptions.IgnoreCase),
        
        // Título - Capítulo X - Título del Capítulo.ext
        new Regex(@"^(?<title>.+?)\s*-\s*(?:Chapter|Ch\.?|c)\s*(?<chapter>[\d.]+)(?:\s*-\s*(?<chapterTitle>.+?))?(?:\s*\((?<year>\d{4})\))?", RegexOptions.IgnoreCase),
        
        // Título vX cY.ext (volumen X capítulo Y)
        new Regex(@"^(?<title>.+?)\s*v(?<volume>\d+)\s*c(?<chapter>[\d.]+)", RegexOptions.IgnoreCase),
        
        // Título - Volumen X Capítulo Y.ext
        new Regex(@"^(?<title>.+?)\s*-\s*Volume\s*(?<volume>\d+)\s*Chapter\s*(?<chapter>[\d.]+)", RegexOptions.IgnoreCase),
        
        // Título Capítulo X.ext (formato simple)
        new Regex(@"^(?<title>.+?)\s*(?:Chapter|Ch\.?|c)\s*(?<chapter>[\d.]+)", RegexOptions.IgnoreCase),
        
        // Título - X.ext (solo número)
        new Regex(@"^(?<title>.+?)\s*-\s*(?<chapter>[\d.]+)", RegexOptions.IgnoreCase)
    };

    public MetadataExtractorService(ILogger<MetadataExtractorService> logger)
    {
        _logger = logger;
    }

    public ExtractedMetadata ExtractFromFilename(string filename)
    {
        var metadata = new ExtractedMetadata();
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(filename);

        _logger.LogDebug("Extracting metadata from filename: {Filename}", filename);

        // Probar cada patrón
        foreach (var pattern in FilenamePatterns)
        {
            var match = pattern.Match(nameWithoutExtension);
            if (match.Success)
            {
                metadata.Title = CleanString(match.Groups["title"].Value);
                
                if (match.Groups["chapter"].Success && 
                    double.TryParse(match.Groups["chapter"].Value, out var chapter))
                {
                    metadata.ChapterNumber = chapter;
                }

                if (match.Groups["volume"].Success && 
                    int.TryParse(match.Groups["volume"].Value, out var volume))
                {
                    metadata.VolumeNumber = volume;
                }

                if (match.Groups["chapterTitle"].Success)
                {
                    metadata.ChapterTitle = CleanString(match.Groups["chapterTitle"].Value);
                }

                if (match.Groups["group"].Success)
                {
                    metadata.ScanGroup = CleanString(match.Groups["group"].Value);
                }

                if (match.Groups["year"].Success && 
                    int.TryParse(match.Groups["year"].Value, out var year))
                {
                    metadata.Year = year;
                }

                _logger.LogInformation(
                    "Extracted metadata - Title: {Title}, Chapter: {Chapter}, Volume: {Volume}", 
                    metadata.Title, 
                    metadata.ChapterNumber, 
                    metadata.VolumeNumber);

                return metadata;
            }
        }

        // Respaldo: usar nombre de archivo como título
        metadata.Title = CleanString(nameWithoutExtension);
        _logger.LogWarning("Could not parse filename pattern, using as-is: {Title}", metadata.Title);

        return metadata;
    }

    public ExtractedMetadata ExtractFromDirectory(string directoryPath)
    {
        var metadata = new ExtractedMetadata();
        var dirInfo = new DirectoryInfo(directoryPath);

        // Intentar extraer del nombre del directorio
        var dirMetadata = ExtractFromFilename(dirInfo.Name);
        metadata.Title = dirMetadata.Title;
        metadata.ChapterNumber = dirMetadata.ChapterNumber;
        metadata.VolumeNumber = dirMetadata.VolumeNumber;

        // Buscar archivos de metadatos (ComicInfo.xml, series.json, etc.)
        var comicInfoPath = Path.Combine(directoryPath, "ComicInfo.xml");
        if (File.Exists(comicInfoPath))
        {
            try
            {
                var comicInfo = ParseComicInfo(comicInfoPath);
                MergeMetadata(metadata, comicInfo);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error parsing ComicInfo.xml at {Path}", comicInfoPath);
            }
        }

        return metadata;
    }

    public bool ValidateMetadata(ExtractedMetadata metadata)
    {
        if (string.IsNullOrWhiteSpace(metadata.Title))
        {
            _logger.LogWarning("Metadata validation failed: Title is required");
            return false;
        }

        if (metadata.ChapterNumber.HasValue && metadata.ChapterNumber.Value < 0)
        {
            _logger.LogWarning("Metadata validation failed: Invalid chapter number");
            return false;
        }

        if (metadata.VolumeNumber.HasValue && metadata.VolumeNumber.Value < 0)
        {
            _logger.LogWarning("Metadata validation failed: Invalid volume number");
            return false;
        }

        if (metadata.Year.HasValue && (metadata.Year.Value < 1900 || metadata.Year.Value > DateTime.UtcNow.Year + 1))
        {
            _logger.LogWarning("Metadata validation failed: Invalid year");
            return false;
        }

        return true;
    }

    private ExtractedMetadata ParseComicInfo(string filePath)
    {
        // Análisis básico de ComicInfo.xml
        // En producción, usar XDocument para análisis XML apropiado
        var metadata = new ExtractedMetadata();
        var content = File.ReadAllText(filePath);

        // Extracción simple basada en regex (reemplazar con análisis XML apropiado)
        var titleMatch = Regex.Match(content, @"<Title>(.+?)</Title>");
        if (titleMatch.Success)
        {
            metadata.Title = titleMatch.Groups[1].Value;
        }

        var seriesMatch = Regex.Match(content, @"<Series>(.+?)</Series>");
        if (seriesMatch.Success && string.IsNullOrEmpty(metadata.Title))
        {
            metadata.Title = seriesMatch.Groups[1].Value;
        }

        var numberMatch = Regex.Match(content, @"<Number>(.+?)</Number>");
        if (numberMatch.Success && double.TryParse(numberMatch.Groups[1].Value, out var number))
        {
            metadata.ChapterNumber = number;
        }

        var volumeMatch = Regex.Match(content, @"<Volume>(.+?)</Volume>");
        if (volumeMatch.Success && int.TryParse(volumeMatch.Groups[1].Value, out var volume))
        {
            metadata.VolumeNumber = volume;
        }

        var writerMatch = Regex.Match(content, @"<Writer>(.+?)</Writer>");
        if (writerMatch.Success)
        {
            metadata.Author = writerMatch.Groups[1].Value;
        }

        var yearMatch = Regex.Match(content, @"<Year>(.+?)</Year>");
        if (yearMatch.Success && int.TryParse(yearMatch.Groups[1].Value, out var year))
        {
            metadata.Year = year;
        }

        return metadata;
    }

    private void MergeMetadata(ExtractedMetadata target, ExtractedMetadata source)
    {
        target.Title ??= source.Title;
        target.ChapterNumber ??= source.ChapterNumber;
        target.VolumeNumber ??= source.VolumeNumber;
        target.ChapterTitle ??= source.ChapterTitle;
        target.Author ??= source.Author;
        target.Year ??= source.Year;
        target.ScanGroup ??= source.ScanGroup;
    }

    private static string CleanString(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Eliminar espacios en blanco extra
        input = Regex.Replace(input, @"\s+", " ").Trim();
        
        // Eliminar artefactos comunes
        input = input.Replace("_", " ");
        
        return input;
    }
}

public class ExtractedMetadata
{
    public string? Title { get; set; }
    public double? ChapterNumber { get; set; }
    public int? VolumeNumber { get; set; }
    public string? ChapterTitle { get; set; }
    public string? Author { get; set; }
    public string? Artist { get; set; }
    public int? Year { get; set; }
    public string? ScanGroup { get; set; }
    public List<string> Tags { get; set; } = new();
    public List<string> Genres { get; set; } = new();
}
