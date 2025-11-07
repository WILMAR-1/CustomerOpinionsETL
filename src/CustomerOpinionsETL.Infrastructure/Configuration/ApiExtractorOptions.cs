namespace CustomerOpinionsETL.Infrastructure.Configuration;

/// <summary>
/// Opciones de configuración para el extractor de API REST
/// </summary>
public class ApiExtractorOptions
{
    public const string SectionName = "ApiExtractor";

    public string BaseUrl { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string? ApiKey { get; set; }
    public int TimeoutSeconds { get; set; } = 300; // 5 minutos default
    public int MaxRetries { get; set; } = 3;
}
