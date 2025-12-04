using MediatR;

namespace CustomerOpinionsETL.Application.Features.Opinions.Queries.ExportOpinions;

/// <summary>
/// Query para exportar opiniones del Data Warehouse a formato CSV
/// Proceso inverso al ETL: DW → CSV
/// </summary>
public record ExportOpinionsQuery : IRequest<ExportOpinionsResult>
{
    // Límite de registros a exportar (opcional)
    // Ejemplo: Limit=10000 exporta los primeros 10,000 registros
    public int? Limit { get; init; }

    // Filtros de fecha (rango)
    public DateTime? DateFrom { get; init; }
    public DateTime? DateTo { get; init; }

    // Filtros de Producto
    public string? ProductName { get; init; }
    public string? ProductCategory { get; init; }
    public string? ProductBrand { get; init; }

    // Filtros de Cliente
    public string? CustomerName { get; init; }
    public string? Country { get; init; }
    public string? City { get; init; }
    public string? Segment { get; init; }

    // Filtros de Canal
    public string? ChannelName { get; init; }
    public string? ChannelType { get; init; }

    // Filtros de Métricas
    public int? RatingMin { get; init; }
    public int? RatingMax { get; init; }
    public int? SentimentScoreMin { get; init; }
    public int? SentimentScoreMax { get; init; }

    // Ordenamiento
    public string OrderBy { get; init; } = "DateKey";
    public string OrderDirection { get; init; } = "desc";
}
