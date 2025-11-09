namespace CustomerOpinionsETL.Application.Common.Models.DTOs;

/// <summary>
/// DTO optimizado para lectura de opiniones desde el Data Warehouse
/// Diseñado para máximo rendimiento: Solo campos necesarios, proyección directa desde SQL
/// </summary>
public class OpinionReadDto
{
    // Hechos (Fact Table)
    public int ProductKey { get; set; }
    public int CustomerKey { get; set; }
    public int DateKey { get; set; }
    public int ChannelKey { get; set; }
    public int? Rating { get; set; }
    public int SentimentScore { get; set; }
    public int OpinionCount { get; set; }

    // Dimensión Producto (desnormalizado para performance)
    public string? ProductName { get; set; }
    public string? ProductCategory { get; set; }
    public string? ProductBrand { get; set; }

    // Dimensión Cliente (desnormalizado)
    public string? CustomerName { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? Segment { get; set; }
    public string? AgeRange { get; set; }

    // Dimensión Fecha (desnormalizado)
    public DateTime FullDate { get; set; }
    public int Year { get; set; }
    public int MonthNumber { get; set; }
    public string? MonthName { get; set; }
    public int Quarter { get; set; }

    // Dimensión Canal (desnormalizado)
    public string? ChannelName { get; set; }
    public string? ChannelType { get; set; }
}
