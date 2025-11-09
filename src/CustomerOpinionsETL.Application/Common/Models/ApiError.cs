namespace CustomerOpinionsETL.Application.Common.Models;

/// <summary>
/// Modelo de error estandarizado para respuestas de API
/// </summary>
public class ApiError
{
    public string Type { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public List<ErrorDetail> Details { get; set; } = new();
    public string SourceService { get; set; } = "opinions-etl-service";
}