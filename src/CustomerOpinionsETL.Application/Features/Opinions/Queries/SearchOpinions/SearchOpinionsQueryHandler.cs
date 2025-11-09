using System.Diagnostics;
using CustomerOpinionsETL.Application.Common.Interfaces;
using CustomerOpinionsETL.Application.Common.Models;
using CustomerOpinionsETL.Application.Common.Models.DTOs;
using CustomerOpinionsETL.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CustomerOpinionsETL.Application.Features.Opinions.Queries.SearchOpinions;

/// <summary>
/// Handler optimizado para búsqueda de opiniones
/// OBJETIVO: 500K+ registros en menos de 5 segundos
///
/// OPTIMIZACIONES APLICADAS:
/// 1. AsNoTracking() - Sin tracking de EF Core
/// 2. Proyección directa a DTO en SQL - Evita materialización de entidades
/// 3. Include solo datos necesarios - JOINs mínimos
/// 4. Paginación aplicada en SQL - Skip/Take
/// 5. Filtros aplicados antes de Count - Reduce registros
/// 6. Ordenamiento en índices - Usa índices de BD
/// </summary>
public class SearchOpinionsQueryHandler : IRequestHandler<SearchOpinionsQuery, ServiceResponse<PaginatedOpinionsDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<SearchOpinionsQueryHandler> _logger;

    public SearchOpinionsQueryHandler(
        IApplicationDbContext context,
        ILogger<SearchOpinionsQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ServiceResponse<PaginatedOpinionsDto>> Handle(
        SearchOpinionsQuery request,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Iniciando búsqueda de opiniones. Page: {Page}, PageSize: {PageSize}",
                request.Page, request.PageSize);

            // PASO 1: Construir query base con JOINs
            var baseQuery = _context.FactOpinions
                .AsNoTracking()  // CRÍTICO: Sin tracking para performance
                .Include(f => f.Product)
                .Include(f => f.Customer)
                .Include(f => f.Date)
                .Include(f => f.Channel)
                .AsQueryable();

            // PASO 2: Aplicar filtros (en SQL, no en memoria)
            baseQuery = ApplyFilters(baseQuery, request);

            // PASO 3: Contar total ANTES de paginación (con filtros aplicados)
            var totalCount = await baseQuery.CountAsync(cancellationToken);

            if (totalCount == 0)
            {
                return ServiceResponse<PaginatedOpinionsDto>.SuccessResult(
                    new PaginatedOpinionsDto
                    {
                        Opinions = new List<OpinionReadDto>(),
                        TotalCount = 0,
                        Page = request.Page,
                        PageSize = request.PageSize,
                        TotalPages = 0,
                        HasNextPage = false,
                        HasPreviousPage = false,
                        QueryTimeMs = stopwatch.Elapsed.TotalMilliseconds
                    },
                    "No se encontraron opiniones con los filtros especificados");
            }

            // PASO 4: Proyección directa a DTO (SELECT en SQL)
            // Esto es CRÍTICO para performance: EF traduce esto a SELECT específico

            // Si se especifica Limit, usar ese valor directamente sin paginación
            var query = baseQuery.OrderByDynamic(request.OrderBy, request.OrderDirection);

            if (request.Limit.HasValue)
            {
                // Modo Limit: retorna los primeros N registros
                query = query.Take(request.Limit.Value);
            }
            else
            {
                // Modo Paginación tradicional
                query = query
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize);
            }

            var opinions = await query
                .Select(f => new OpinionReadDto
                {
                    // Hechos
                    ProductKey = f.ProductKey,
                    CustomerKey = f.CustomerKey,
                    DateKey = f.DateKey,
                    ChannelKey = f.ChannelKey,
                    Rating = f.Rating,
                    SentimentScore = f.SentimentScore,
                    OpinionCount = f.OpinionCount,

                    // Producto (desnormalizado)
                    ProductName = f.Product!.ProductName,
                    ProductCategory = f.Product.ProductCategory,
                    ProductBrand = f.Product.ProductBrand,

                    // Cliente (desnormalizado)
                    CustomerName = f.Customer!.CustomerName,
                    Country = f.Customer.Country,
                    City = f.Customer.City,
                    Segment = f.Customer.Segment,
                    AgeRange = f.Customer.AgeRange,

                    // Fecha (desnormalizado)
                    FullDate = f.Date!.FullDate,
                    Year = f.Date.Year,
                    MonthNumber = f.Date.MonthNumber,
                    MonthName = f.Date.MonthName,
                    Quarter = f.Date.Quarter,

                    // Canal (desnormalizado)
                    ChannelName = f.Channel!.ChannelName,
                    ChannelType = f.Channel.ChannelType
                })
                .ToListAsync(cancellationToken);

            stopwatch.Stop();

            var totalPages = request.Limit.HasValue
                ? 1
                : (int)Math.Ceiling((double)totalCount / request.PageSize);

            var actualPageSize = request.Limit ?? request.PageSize;

            var result = new PaginatedOpinionsDto
            {
                Opinions = opinions,
                TotalCount = totalCount,
                Page = request.Limit.HasValue ? 1 : request.Page,
                PageSize = actualPageSize,
                TotalPages = totalPages,
                HasNextPage = request.Limit.HasValue ? false : (request.Page < totalPages),
                HasPreviousPage = request.Limit.HasValue ? false : (request.Page > 1),
                QueryTimeMs = stopwatch.Elapsed.TotalMilliseconds
            };

            _logger.LogInformation(
                "Búsqueda completada. {Count} registros encontrados (página {Page} de {TotalPages}). Tiempo: {ElapsedMs}ms",
                totalCount, request.Page, totalPages, stopwatch.Elapsed.TotalMilliseconds);

            return ServiceResponse<PaginatedOpinionsDto>.SuccessResult(
                result,
                $"{opinions.Count} opiniones obtenidas exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar opiniones");
            return ServiceResponse<PaginatedOpinionsDto>.FailureResult(
                "Error al buscar opiniones",
                "INTERNAL_ERROR");
        }
    }

    private IQueryable<FactOpinion> ApplyFilters(
        IQueryable<FactOpinion> query,
        SearchOpinionsQuery request)
    {
        // Filtros de Producto
        if (request.ProductKey.HasValue)
            query = query.Where(f => f.ProductKey == request.ProductKey.Value);

        if (!string.IsNullOrEmpty(request.ProductName))
            query = query.Where(f => f.Product!.ProductName!.Contains(request.ProductName));

        if (!string.IsNullOrEmpty(request.ProductCategory))
            query = query.Where(f => f.Product!.ProductCategory == request.ProductCategory);

        if (!string.IsNullOrEmpty(request.ProductBrand))
            query = query.Where(f => f.Product!.ProductBrand == request.ProductBrand);

        // Filtros de Cliente
        if (request.CustomerKey.HasValue)
            query = query.Where(f => f.CustomerKey == request.CustomerKey.Value);

        if (!string.IsNullOrEmpty(request.CustomerName))
            query = query.Where(f => f.Customer!.CustomerName!.Contains(request.CustomerName));

        if (!string.IsNullOrEmpty(request.Country))
            query = query.Where(f => f.Customer!.Country == request.Country);

        if (!string.IsNullOrEmpty(request.City))
            query = query.Where(f => f.Customer!.City == request.City);

        if (!string.IsNullOrEmpty(request.Segment))
            query = query.Where(f => f.Customer!.Segment == request.Segment);

        // Filtros de Canal
        if (request.ChannelKey.HasValue)
            query = query.Where(f => f.ChannelKey == request.ChannelKey.Value);

        if (!string.IsNullOrEmpty(request.ChannelName))
            query = query.Where(f => f.Channel!.ChannelName == request.ChannelName);

        // Filtros de Fecha
        if (request.DateFrom.HasValue)
        {
            var dateKeyFrom = int.Parse(request.DateFrom.Value.ToString("yyyyMMdd"));
            query = query.Where(f => f.DateKey >= dateKeyFrom);
        }

        if (request.DateTo.HasValue)
        {
            var dateKeyTo = int.Parse(request.DateTo.Value.ToString("yyyyMMdd"));
            query = query.Where(f => f.DateKey <= dateKeyTo);
        }

        if (request.Year.HasValue)
            query = query.Where(f => f.Date!.Year == request.Year.Value);

        if (request.Month.HasValue)
            query = query.Where(f => f.Date!.MonthNumber == request.Month.Value);

        if (request.Quarter.HasValue)
            query = query.Where(f => f.Date!.Quarter == request.Quarter.Value);

        // Filtros de Métricas
        if (request.RatingMin.HasValue)
            query = query.Where(f => f.Rating >= request.RatingMin.Value);

        if (request.RatingMax.HasValue)
            query = query.Where(f => f.Rating <= request.RatingMax.Value);

        if (request.SentimentScoreMin.HasValue)
            query = query.Where(f => f.SentimentScore >= request.SentimentScoreMin.Value);

        if (request.SentimentScoreMax.HasValue)
            query = query.Where(f => f.SentimentScore <= request.SentimentScoreMax.Value);

        return query;
    }
}

// Extension method para ordenamiento dinámico
public static class QueryableExtensions
{
    public static IQueryable<FactOpinion> OrderByDynamic(
        this IQueryable<FactOpinion> query,
        string orderBy,
        string direction)
    {
        var isDescending = direction.Equals("desc", StringComparison.OrdinalIgnoreCase);

        return orderBy.ToLower() switch
        {
            "datekey" => isDescending ? query.OrderByDescending(f => f.DateKey) : query.OrderBy(f => f.DateKey),
            "rating" => isDescending ? query.OrderByDescending(f => f.Rating) : query.OrderBy(f => f.Rating),
            "sentimentscore" => isDescending ? query.OrderByDescending(f => f.SentimentScore) : query.OrderBy(f => f.SentimentScore),
            "productkey" => isDescending ? query.OrderByDescending(f => f.ProductKey) : query.OrderBy(f => f.ProductKey),
            "customerkey" => isDescending ? query.OrderByDescending(f => f.CustomerKey) : query.OrderBy(f => f.CustomerKey),
            _ => query.OrderByDescending(f => f.DateKey)  // Default
        };
    }
}
