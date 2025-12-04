namespace CustomerOpinionsETL.Application.Features.Opinions.Queries.ExportOpinions;

/// <summary>
/// Resultado de la exportación a CSV
/// </summary>
public class ExportOpinionsResult
{
    public bool Success { get; set; }
    public byte[] CsvData { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
    public int TotalRecords { get; set; }
    public double ExportTimeMs { get; set; }
    public string? ErrorMessage { get; set; }
}
