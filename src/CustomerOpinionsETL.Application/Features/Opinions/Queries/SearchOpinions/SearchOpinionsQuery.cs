using CustomerOpinionsETL.Application.Common.Models;
using CustomerOpinionsETL.Application.Common.Models.DTOs;
using MediatR;

namespace CustomerOpinionsETL.Application.Features.Opinions.Queries.SearchOpinions;

/// <summary>
/// Query optimizada para búsqueda de opiniones con filtros y paginación
/// Objetivo: 500K+ registros en menos de 5 segundos
/// </summary>
public record SearchOpinionsQuery : IRequest<ServiceResponse<PaginatedOpinionsDto>>
{
    // Paginación (OBLIGATORIA para performance)
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;

    // Límite directo de registros (alternativa a paginación)
    // Ejemplo: Limit=500000 retorna los primeros 500,000 registros
    // Si se especifica, ignora Page y PageSize
    public int? Limit { get; init; }

    // Filtros de búsqueda
    public int? ProductKey { get; init; }
    public string? ProductName { get; init; }
    public string? ProductCategory { get; init; }
    public string? ProductBrand { get; init; }

    public int? CustomerKey { get; init; }
    public string? CustomerName { get; init; }
    public string? Country { get; init; }
    public string? City { get; init; }
    public string? Segment { get; init; }

    public int? ChannelKey { get; init; }
    public string? ChannelName { get; init; }

    // Filtros de fecha
    public DateTime? DateFrom { get; init; }
    public DateTime? DateTo { get; init; }
    public int? Year { get; init; }
    public int? Month { get; init; }
    public int? Quarter { get; init; }

    // Filtros de métricas
    public int? RatingMin { get; init; }
    public int? RatingMax { get; init; }
    public int? SentimentScoreMin { get; init; }
    public int? SentimentScoreMax { get; init; }

    // Ordenamiento
    public string OrderBy { get; init; } = "DateKey";
    public string OrderDirection { get; init; } = "desc";

    // Búsqueda de texto (para uso futuro)
    public string? SearchTerm { get; init; }
}
