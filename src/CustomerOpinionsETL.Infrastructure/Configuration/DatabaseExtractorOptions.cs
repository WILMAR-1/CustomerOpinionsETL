namespace CustomerOpinionsETL.Infrastructure.Configuration;

/// <summary>
/// Opciones de configuración para el extractor de base de datos relacional
/// </summary>
public class DatabaseExtractorOptions
{
    public const string SectionName = "DatabaseExtractor";

    public string ConnectionString { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public int CommandTimeout { get; set; } = 300; // 5 minutos default
}
