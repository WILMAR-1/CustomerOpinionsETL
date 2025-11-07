namespace CustomerOpinionsETL.Infrastructure.Configuration;

/// <summary>
/// Opciones de configuración para el extractor de CSV
/// </summary>
public class CsvExtractorOptions
{
    public const string SectionName = "CsvExtractor";

    public string FilePath { get; set; } = string.Empty;
    public bool HasHeaderRecord { get; set; } = true;
    public string Delimiter { get; set; } = ",";
}
