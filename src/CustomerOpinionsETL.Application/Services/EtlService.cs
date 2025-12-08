using System.Collections.Concurrent;
using System.Diagnostics;
using CustomerOpinionsETL.Application.Configuration;
using CustomerOpinionsETL.Domain.Entities;
using CustomerOpinionsETL.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CustomerOpinionsETL.Application.Services;

/// <summary>
/// Servicio principal que orquesta el proceso ETL completo
/// Coordina la extracción desde múltiples fuentes y la carga en el data warehouse
/// </summary>
public class EtlService
{
    private readonly IEnumerable<IExtractor> _extractors;
    private readonly IDataLoader _dataLoader;
    private readonly ILogger<EtlService> _logger;
    private readonly EtlOptions _options;

    public EtlService(
        IEnumerable<IExtractor> extractors,
        IDataLoader dataLoader,
        ILogger<EtlService> logger,
        IOptions<EtlOptions> options)
    {
        _extractors = extractors;
        _dataLoader = dataLoader;
        _logger = logger;
        _options = options.Value;
    }

    /// <summary>
    /// Ejecuta el proceso ETL completo
    /// </summary>
    public async Task<EtlResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("========================================");
        _logger.LogInformation("Iniciando proceso ETL");
        _logger.LogInformation("========================================");

        var overallStopwatch = Stopwatch.StartNew();
        var result = new EtlResult();

        try
        {
            // FASE 1: EXTRACCIÓN
            var allOpinions = await ExtractFromAllSourcesAsync(cancellationToken);
            result.TotalRecordsExtracted = allOpinions.Count;

            _logger.LogInformation(
                "Extracción completada: {Count} registros extraídos de {Sources} fuentes",
                allOpinions.Count, _extractors.Count());

            // FASE 2: TRANSFORMACIÓN (validación básica)
            var validOpinions = TransformAndValidate(allOpinions);
            result.TotalRecordsTransformed = validOpinions.Count;
            result.TotalRecordsRejected = allOpinions.Count - validOpinions.Count;

            _logger.LogInformation(
                "Transformación completada: {Valid} registros válidos, {Rejected} rechazados",
                validOpinions.Count, result.TotalRecordsRejected);

            // FASE 3: LIMPIEZA DE FACT TABLES (antes de cargar)
            _logger.LogInformation("Limpiando tabla de hechos antes de la carga...");
            var deletedRecords = await _dataLoader.TruncateFactTableAsync(cancellationToken);
            _logger.LogInformation("Registros eliminados de Fact_Opinions: {Count}", deletedRecords);

            // FASE 4: CARGA
            if (validOpinions.Any())
            {
                result.TotalRecordsLoaded = await _dataLoader.LoadOpinionsAsync(
                    validOpinions,
                    cancellationToken);

                _logger.LogInformation(
                    "Carga completada: {Count} registros insertados en el data warehouse",
                    result.TotalRecordsLoaded);
            }
            else
            {
                _logger.LogWarning("No hay registros válidos para cargar");
            }

            overallStopwatch.Stop();
            result.TotalElapsedTime = overallStopwatch.Elapsed;
            result.Success = true;

            _logger.LogInformation("========================================");
            _logger.LogInformation("Proceso ETL completado exitosamente");
            _logger.LogInformation("Tiempo total: {Elapsed}", result.TotalElapsedTime);
            _logger.LogInformation("Rendimiento: {Rate:F2} registros/segundo",
                result.TotalRecordsLoaded / result.TotalElapsedTime.TotalSeconds);
            _logger.LogInformation("========================================");

            return result;
        }
        catch (Exception ex)
        {
            overallStopwatch.Stop();
            result.TotalElapsedTime = overallStopwatch.Elapsed;
            result.Success = false;
            result.ErrorMessage = ex.Message;

            _logger.LogError(ex, "Error crítico durante el proceso ETL");
            _logger.LogInformation("========================================");

            throw;
        }
    }

    private async Task<List<OpinionDto>> ExtractFromAllSourcesAsync(
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando extracción desde {Count} fuentes", _extractors.Count());

        var allOpinions = new List<OpinionDto>();

        if (_options.EnableParallelExtraction)
        {
            _logger.LogInformation("Ejecutando extracción en paralelo (MaxDegreeOfParallelism: {Degree})",
                _options.MaxDegreeOfParallelism);

            // Extracción paralela para máximo rendimiento
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = _options.MaxDegreeOfParallelism,
                CancellationToken = cancellationToken
            };

            var extractionTasks = _extractors.Select(async extractor =>
            {
                try
                {
                    _logger.LogInformation("Iniciando extracción: {ExtractorName}", extractor.ExtractorName);
                    var stopwatch = Stopwatch.StartNew();

                    var opinions = await extractor.ExtractAsync(cancellationToken);
                    var opinionsList = opinions.ToList();

                    stopwatch.Stop();
                    _logger.LogInformation(
                        "{ExtractorName} completado: {Count} registros en {Elapsed}ms",
                        extractor.ExtractorName, opinionsList.Count, stopwatch.ElapsedMilliseconds);

                    return opinionsList;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error en extractor {ExtractorName}. Continuando con otras fuentes.",
                        extractor.ExtractorName);
                    return new List<OpinionDto>();
                }
            });

            var results = await Task.WhenAll(extractionTasks);
            allOpinions.AddRange(results.SelectMany(r => r));
        }
        else
        {
            _logger.LogInformation("Ejecutando extracción secuencial");

            // Extracción secuencial
            foreach (var extractor in _extractors)
            {
                try
                {
                    _logger.LogInformation("Iniciando extracción: {ExtractorName}", extractor.ExtractorName);
                    var stopwatch = Stopwatch.StartNew();

                    var opinions = await extractor.ExtractAsync(cancellationToken);
                    var opinionsList = opinions.ToList();

                    stopwatch.Stop();
                    _logger.LogInformation(
                        "{ExtractorName} completado: {Count} registros en {Elapsed}ms",
                        extractor.ExtractorName, opinionsList.Count, stopwatch.ElapsedMilliseconds);

                    allOpinions.AddRange(opinionsList);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error en extractor {ExtractorName}. Continuando con otras fuentes.",
                        extractor.ExtractorName);
                }
            }
        }

        return allOpinions;
    }

    private List<OpinionDto> TransformAndValidate(List<OpinionDto> opinions)
    {
        _logger.LogInformation("Aplicando transformaciones y validaciones en paralelo");

        // Usar ConcurrentBag para thread-safety en operaciones paralelas
        var validOpinions = new ConcurrentBag<OpinionDto>();

        // Contadores atómicos para estadísticas de rechazo
        var rejectedProductName = 0;
        var rejectedCustomerName = 0;
        var rejectedDate = 0;
        var normalizedSentiment = 0;

        // Procesar en paralelo para máximo rendimiento
        // NOTA: Se evita logging individual dentro del bucle paralelo para evitar
        // problemas de contención y sincronización que degradan el rendimiento
        Parallel.ForEach(opinions, new ParallelOptions
        {
            MaxDegreeOfParallelism = _options.MaxDegreeOfParallelism
        },
        opinion =>
        {
            // Validaciones básicas
            if (string.IsNullOrWhiteSpace(opinion.ProductName))
            {
                Interlocked.Increment(ref rejectedProductName);
                return;
            }

            if (string.IsNullOrWhiteSpace(opinion.CustomerName))
            {
                Interlocked.Increment(ref rejectedCustomerName);
                return;
            }

            if (opinion.OpinionDate == default)
            {
                Interlocked.Increment(ref rejectedDate);
                return;
            }

            // Validar rango de sentiment score (-1, 0, 1)
            if (opinion.SentimentScore < -1 || opinion.SentimentScore > 1)
            {
                Interlocked.Increment(ref normalizedSentiment);
                opinion.SentimentScore = opinion.SentimentScore > 0 ? 1 : -1;
            }

            validOpinions.Add(opinion);
        });

        // Log consolidado de rechazos (evita contención durante procesamiento paralelo)
        if (rejectedProductName > 0)
            _logger.LogWarning("Opiniones rechazadas por ProductName vacío: {Count}", rejectedProductName);
        if (rejectedCustomerName > 0)
            _logger.LogWarning("Opiniones rechazadas por CustomerName vacío: {Count}", rejectedCustomerName);
        if (rejectedDate > 0)
            _logger.LogWarning("Opiniones rechazadas por fecha inválida: {Count}", rejectedDate);
        if (normalizedSentiment > 0)
            _logger.LogInformation("Sentiment scores normalizados: {Count}", normalizedSentiment);

        // Convertir ConcurrentBag a List para mantener compatibilidad
        return validOpinions.ToList();
    }
}

/// <summary>
/// Resultado del proceso ETL
/// </summary>
public class EtlResult
{
    public bool Success { get; set; }
    public int TotalRecordsExtracted { get; set; }
    public int TotalRecordsTransformed { get; set; }
    public int TotalRecordsLoaded { get; set; }
    public int TotalRecordsRejected { get; set; }
    public TimeSpan TotalElapsedTime { get; set; }
    public string? ErrorMessage { get; set; }
}
