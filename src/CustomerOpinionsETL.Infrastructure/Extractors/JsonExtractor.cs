using System.Text.Json;
using CustomerOpinionsETL.Domain.Entities;
using CustomerOpinionsETL.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace CustomerOpinionsETL.Infrastructure.Extractors;

/// <summary>
/// NUEVA FUENTE DE DATOS: Extractor JSON
/// NO modifica ningún componente existente - Solo implementa IExtractor
/// </summary>
public class JsonExtractor : IExtractor
{
    private readonly ILogger<JsonExtractor> _logger;
    private readonly string _filePath;

    public string ExtractorName => "JSON Extractor";

    public JsonExtractor(ILogger<JsonExtractor> logger)
    {
        _logger = logger;
        _filePath = "Data/opinions.json"; // Ejemplo
    }

    public async Task<IEnumerable<OpinionDto>> ExtractAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Iniciando extracción de JSON desde: {FilePath}", _filePath);

        try
        {
            var jsonContent = await File.ReadAllTextAsync(_filePath, cancellationToken);
            var opinions = JsonSerializer.Deserialize<List<OpinionDto>>(jsonContent);

            _logger.LogInformation("JSON extraído: {Count} registros", opinions?.Count ?? 0);
            return opinions ?? new List<OpinionDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al extraer datos del JSON");
            throw;
        }
    }
}
