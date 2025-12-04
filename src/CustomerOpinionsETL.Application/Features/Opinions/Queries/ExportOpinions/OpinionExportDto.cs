namespace CustomerOpinionsETL.Application.Features.Opinions.Queries.ExportOpinions;

/// <summary>
/// DTO para exportación de opiniones a CSV con todos los datos desnormalizados
/// </summary>
public class OpinionExportDto
{
    // Producto
    public string ProductName { get; set; } = string.Empty;
    public string? ProductCategory { get; set; }
    public string? ProductBrand { get; set; }

    // Cliente
    public string CustomerName { get; set; } = string.Empty;
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? Segment { get; set; }
    public string? AgeRange { get; set; }

    // Fecha
    public DateTime OpinionDate { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public string? MonthName { get; set; }
    public int Quarter { get; set; }

    // Canal
    public string ChannelName { get; set; } = string.Empty;
    public string? ChannelType { get; set; }

    // Métricas
    public int? Rating { get; set; }
    public int SentimentScore { get; set; }
    public int OpinionCount { get; set; }
}
