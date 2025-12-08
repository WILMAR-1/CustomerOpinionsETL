using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using CustomerOpinionsETL.Domain.Entities;
using CustomerOpinionsETL.Domain.Interfaces;
using CustomerOpinionsETL.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CustomerOpinionsETL.Infrastructure.Extractors;

/// <summary>
/// Extractor de datos desde archivos CSV (Encuestas Internas)
/// </summary>
public class CsvExtractor : IExtractor
{
    private readonly CsvExtractorOptions _options;
    private readonly ILogger<CsvExtractor> _logger;

    public string ExtractorName => "CSV Extractor";

    public CsvExtractor(
        IOptions<CsvExtractorOptions> options,
        ILogger<CsvExtractor> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IEnumerable<OpinionDto>> ExtractAsync(CancellationToken cancellationToken = default)
    {
        // Construir ruta absoluta si es relativa
        var filePath = _options.FilePath;
        if (!Path.IsPathRooted(filePath))
        {
            var baseDir = AppContext.BaseDirectory;

            // Primero intentar desde el directorio base (bin/Debug/net9.0)
            // Esto funciona cuando el archivo se copia al output durante el build
            var localPath = Path.Combine(baseDir, filePath);

            if (File.Exists(localPath))
            {
                filePath = localPath;
            }
            else
            {
                // Si no existe localmente, navegar desde bin/Debug/net9.0 hasta el directorio del proyecto
                var projectDir = Directory.GetParent(baseDir)?.Parent?.Parent?.Parent?.FullName ?? baseDir;
                filePath = Path.Combine(projectDir, filePath);
            }
        }

        _logger.LogInformation("Iniciando extracción de CSV desde: {FilePath}", filePath);
        var startTime = DateTime.UtcNow;

        try
        {
            if (!File.Exists(filePath))
            {
                _logger.LogError("El archivo CSV no existe: {FilePath}", filePath);
                _logger.LogInformation("Ruta base: {BaseDir}", AppContext.BaseDirectory);
                throw new FileNotFoundException($"No se encontró el archivo: {filePath}");
            }

            // Optimizado: Pre-alocar lista con capacidad estimada basada en tamaño del archivo
            var fileInfo = new FileInfo(filePath);
            var estimatedRecords = (int)(fileInfo.Length / 100); // ~100 bytes por registro promedio
            var opinions = new List<OpinionDto>(estimatedRecords);

            // Optimizado: Usar StreamReader con buffer grande para lectura eficiente
            using var reader = new StreamReader(filePath, System.Text.Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 65536);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = _options.HasHeaderRecord,
                Delimiter = _options.Delimiter,
                MissingFieldFound = null,
                BadDataFound = null,
                BufferSize = 65536 // Buffer grande para mejor rendimiento
            });

            // Lectura streaming sin cargar todo en memoria
            await foreach (var record in csv.GetRecordsAsync<CsvOpinionDto>(cancellationToken))
            {
                if (record != null)
                {
                    opinions.Add(MapToOpinionDto(record));
                }
            }

            var elapsed = DateTime.UtcNow - startTime;
            _logger.LogInformation(
                "Extracción de CSV completada. {Count} registros extraídos en {ElapsedMs}ms",
                opinions.Count, elapsed.TotalMilliseconds);

            return opinions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al extraer datos del CSV: {FilePath}", _options.FilePath);
            throw;
        }
    }

    private OpinionDto MapToOpinionDto(CsvOpinionDto csvDto)
    {
        return new OpinionDto
        {
            SourceProductId = csvDto.ProductId,
            ProductName = csvDto.ProductName,
            ProductCategory = csvDto.Category,
            ProductBrand = null,

            SourceCustomerId = csvDto.CustomerId,
            CustomerName = csvDto.CustomerName,
            Country = csvDto.Country,
            City = csvDto.City,
            Segment = "Survey",
            AgeRange = null,

            OpinionDate = csvDto.SurveyDate,
            ChannelName = "Internal Survey",
            ChannelType = "CSV",

            Rating = csvDto.Rating,
            SentimentScore = csvDto.Sentiment,
            CommentText = csvDto.Comment
        };
    }
}
