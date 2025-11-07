using System.Data;
using CustomerOpinionsETL.Domain.Entities;
using CustomerOpinionsETL.Domain.Interfaces;
using CustomerOpinionsETL.Infrastructure.Configuration;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CustomerOpinionsETL.Infrastructure.Extractors;

/// <summary>
/// Extractor de datos desde base de datos relacional
/// Usa ADO.NET para máximo rendimiento
/// </summary>
public class DatabaseExtractor : IExtractor
{
    private readonly DatabaseExtractorOptions _options;
    private readonly ILogger<DatabaseExtractor> _logger;

    public string ExtractorName => "Database Extractor";

    public DatabaseExtractor(
        IOptions<DatabaseExtractorOptions> options,
        ILogger<DatabaseExtractor> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IEnumerable<OpinionDto>> ExtractAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Iniciando extracción de base de datos");
        var startTime = DateTime.UtcNow;

        try
        {
            var opinions = new List<OpinionDto>();

            await using var connection = new SqlConnection(_options.ConnectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = new SqlCommand(_options.Query, connection)
            {
                CommandTimeout = _options.CommandTimeout,
                CommandType = CommandType.Text
            };

            await using var reader = await command.ExecuteReaderAsync(
                CommandBehavior.SequentialAccess,
                cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                var opinion = MapToOpinionDto(reader);
                opinions.Add(opinion);
            }

            var elapsed = DateTime.UtcNow - startTime;
            _logger.LogInformation(
                "Extracción de base de datos completada. {Count} registros extraídos en {ElapsedMs}ms",
                opinions.Count, elapsed.TotalMilliseconds);

            return opinions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al extraer datos de la base de datos");
            throw;
        }
    }

    private OpinionDto MapToOpinionDto(SqlDataReader reader)
    {
        return new OpinionDto
        {
            // Producto
            SourceProductId = reader.GetInt32("ProductId"),
            ProductName = reader.GetString("ProductName"),
            ProductCategory = GetNullableString(reader, "Category"),
            ProductBrand = GetNullableString(reader, "Brand"),

            // Cliente
            SourceCustomerId = reader.GetInt32("CustomerId"),
            CustomerName = reader.GetString("CustomerName"),
            Country = GetNullableString(reader, "Country"),
            City = GetNullableString(reader, "City"),
            Segment = GetNullableString(reader, "Segment"),
            AgeRange = GetNullableString(reader, "AgeRange"),

            // Opinión
            OpinionDate = reader.GetDateTime("ReviewDate"),
            ChannelName = "Web Review",
            ChannelType = "Database",

            Rating = GetNullableInt(reader, "Rating"),
            SentimentScore = reader.GetInt32("SentimentScore"),
            CommentText = GetNullableString(reader, "Comment")
        };
    }

    private static string? GetNullableString(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }

    private static int? GetNullableInt(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal);
    }
}
