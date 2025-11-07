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
        _logger.LogInformation("Iniciando extracción de CSV desde: {FilePath}", _options.FilePath);
        var startTime = DateTime.UtcNow;

        try
        {
            if (!File.Exists(_options.FilePath))
            {
                _logger.LogError("El archivo CSV no existe: {FilePath}", _options.FilePath);
                throw new FileNotFoundException($"No se encontró el archivo: {_options.FilePath}");
            }

            var opinions = new List<OpinionDto>();

            using (var reader = new StreamReader(_options.FilePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = _options.HasHeaderRecord,
                Delimiter = _options.Delimiter,
                MissingFieldFound = null,
                BadDataFound = null
            }))
            {
                var records = csv.GetRecordsAsync<CsvOpinionDto>(cancellationToken);

                await foreach (var record in records.WithCancellation(cancellationToken))
                {
                    var opinion = MapToOpinionDto(record);
                    opinions.Add(opinion);
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
