namespace CustomerOpinionsETL.Application.Common.Models;

/// <summary>
/// Detalle de error individual (por campo o específico)
/// </summary>
public class ErrorDetail
{
    public string Code { get; set; } = string.Empty;
    public string? Field { get; set; }
    public string Message { get; set; } = string.Empty;
}
