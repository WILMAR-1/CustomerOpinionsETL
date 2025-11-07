namespace CustomerOpinionsETL.Application.Configuration;

/// <summary>
/// Opciones generales del proceso ETL
/// </summary>
public class EtlOptions
{
    public const string SectionName = "Etl";

    /// <summary>
    /// Tamaño del lote para procesamiento por lotes (bulk insert)
    /// </summary>
    public int BatchSize { get; set; } = 10000;

    /// <summary>
    /// Grado máximo de paralelismo para procesamiento
    /// </summary>
    public int MaxDegreeOfParallelism { get; set; } = Environment.ProcessorCount;

    /// <summary>
    /// Habilitar procesamiento paralelo de fuentes
    /// </summary>
    public bool EnableParallelExtraction { get; set; } = true;

    /// <summary>
    /// Ruta para archivos temporales de staging
    /// </summary>
    public string StagingPath { get; set; } = "./staging";
}
