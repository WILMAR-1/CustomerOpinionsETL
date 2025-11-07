using CustomerOpinionsETL.Application.Services;

namespace CustomerOpinionsETL.Worker;

/// <summary>
/// Worker Service que ejecuta el proceso ETL
/// Se puede configurar para ejecutarse una vez o de forma recurrente
/// </summary>
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly EtlService _etlService;
    private readonly IConfiguration _configuration;

    public Worker(
        ILogger<Worker> logger,
        EtlService etlService,
        IConfiguration configuration)
    {
        _logger = logger;
        _etlService = etlService;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker Service iniciado en: {Time}", DateTimeOffset.Now);

        try
        {
            // Ejecutar el proceso ETL
            var result = await _etlService.ExecuteAsync(stoppingToken);

            if (result.Success)
            {
                _logger.LogInformation(
                    "Proceso ETL completado exitosamente. " +
                    "Extraídos: {Extracted}, Transformados: {Transformed}, " +
                    "Cargados: {Loaded}, Rechazados: {Rejected}",
                    result.TotalRecordsExtracted,
                    result.TotalRecordsTransformed,
                    result.TotalRecordsLoaded,
                    result.TotalRecordsRejected);
            }
            else
            {
                _logger.LogError(
                    "Proceso ETL falló: {ErrorMessage}",
                    result.ErrorMessage);
            }

            // Si se configura para ejecución recurrente, descomentar lo siguiente:
            /*
            var intervalMinutes = _configuration.GetValue<int>("Etl:IntervalMinutes", 60);
            _logger.LogInformation("Esperando {Minutes} minutos para la próxima ejecución", intervalMinutes);
            await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken);
            */
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Error crítico en Worker Service");
            throw;
        }
    }
}
