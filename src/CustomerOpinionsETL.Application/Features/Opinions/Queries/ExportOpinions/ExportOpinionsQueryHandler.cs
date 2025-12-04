using System.Diagnostics;
using System.Globalization;
using System.Text;
using CustomerOpinionsETL.Application.Common.Interfaces;
using CustomerOpinionsETL.Domain.Entities;
using CsvHelper;
using CsvHelper.Configuration;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CustomerOpinionsETL.Application.Features.Opinions.Queries.ExportOpinions;

/// <summary>
/// Handler para exportar opiniones del Data Warehouse a formato CSV
/// Proceso inverso al ETL: extrae datos del DW y los convierte a CSV
/// </summary>
public class ExportOpinionsQueryHandler : IRequestHandler<ExportOpinionsQuery, ExportOpinionsResult>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ExportOpinionsQueryHandler> _logger;

    public ExportOpinionsQueryHandler(
        IApplicationDbContext context,
        ILogger<ExportOpinionsQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ExportOpinionsResult> Handle(
        ExportOpinionsQuery request,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Iniciando exportación de opiniones a CSV. Limit: {Limit}", request.Limit);

            // PASO 1: Construir query base con JOINs
            var baseQuery = _context.FactOpinions
                .AsNoTracking()
                .Include(f => f.Product)
                .Include(f => f.Customer)
                .Include(f => f.Date)
                .Include(f => f.Channel)
                .AsQueryable();

            // PASO 2: Aplicar filtros
            baseQuery = ApplyFilters(baseQuery, request);

            // PASO 3: Aplicar ordenamiento y límite
            var query = baseQuery.OrderByDynamic(request.OrderBy, request.OrderDirection);

            if (request.Limit.HasValue)
            {
                query = query.Take(request.Limit.Value);
            }

            // PASO 4: Proyectar a DTO de exportación
            var opinions = await query
                .Select(f => new OpinionExportDto
                {
                    // Producto
                    ProductName = f.Product!.ProductName ?? string.Empty,
                    ProductCategory = f.Product.ProductCategory,
                    ProductBrand = f.Product.ProductBrand,

                    // Cliente
                    CustomerName = f.Customer!.CustomerName ?? string.Empty,
                    Country = f.Customer.Country,
                    City = f.Customer.City,
                    Segment = f.Customer.Segment,
                    AgeRange = f.Customer.AgeRange,

                    // Fecha
                    OpinionDate = f.Date!.FullDate,
                    Year = f.Date.Year,
                    Month = f.Date.MonthNumber,
                    MonthName = f.Date.MonthName,
                    Quarter = f.Date.Quarter,

                    // Canal
                    ChannelName = f.Channel!.ChannelName ?? string.Empty,
                    ChannelType = f.Channel.ChannelType,

                    // Métricas
                    Rating = f.Rating,
                    SentimentScore = f.SentimentScore,
                    OpinionCount = f.OpinionCount
                })
                .ToListAsync(cancellationToken);

            if (opinions.Count == 0)
            {
                _logger.LogWarning("No se encontraron opiniones para exportar con los filtros especificados");
                return new ExportOpinionsResult
                {
                    Success = false,
                    ErrorMessage = "No se encontraron opiniones para exportar con los filtros especificados"
                };
            }

            // PASO 5: Generar CSV en memoria
            var csvData = GenerateCsv(opinions);

            stopwatch.Stop();

            var fileName = $"opiniones_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

            _logger.LogInformation(
                "Exportación completada. {Count} registros exportados en {ElapsedMs}ms",
                opinions.Count,
                stopwatch.Elapsed.TotalMilliseconds);

            return new ExportOpinionsResult
            {
                Success = true,
                CsvData = csvData,
                FileName = fileName,
                TotalRecords = opinions.Count,
                ExportTimeMs = stopwatch.Elapsed.TotalMilliseconds
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al exportar opiniones a CSV");
            return new ExportOpinionsResult
            {
                Success = false,
                ErrorMessage = $"Error al exportar opiniones: {ex.Message}"
            };
        }
    }

    private byte[] GenerateCsv(List<OpinionExportDto> opinions)
    {
        using var memoryStream = new MemoryStream();
        using var writer = new StreamWriter(memoryStream, Encoding.UTF8);
        using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            Delimiter = ","
        });

        csv.WriteRecords(opinions);
        writer.Flush();

        return memoryStream.ToArray();
    }

    private IQueryable<FactOpinion> ApplyFilters(
        IQueryable<FactOpinion> query,
        ExportOpinionsQuery request)
    {
        // Filtros de Producto
        if (!string.IsNullOrEmpty(request.ProductName))
            query = query.Where(f => f.Product!.ProductName!.Contains(request.ProductName));

        if (!string.IsNullOrEmpty(request.ProductCategory))
            query = query.Where(f => f.Product!.ProductCategory == request.ProductCategory);

        if (!string.IsNullOrEmpty(request.ProductBrand))
            query = query.Where(f => f.Product!.ProductBrand == request.ProductBrand);

        // Filtros de Cliente
        if (!string.IsNullOrEmpty(request.CustomerName))
            query = query.Where(f => f.Customer!.CustomerName!.Contains(request.CustomerName));

        if (!string.IsNullOrEmpty(request.Country))
            query = query.Where(f => f.Customer!.Country == request.Country);

        if (!string.IsNullOrEmpty(request.City))
            query = query.Where(f => f.Customer!.City == request.City);

        if (!string.IsNullOrEmpty(request.Segment))
            query = query.Where(f => f.Customer!.Segment == request.Segment);

        // Filtros de Canal
        if (!string.IsNullOrEmpty(request.ChannelName))
            query = query.Where(f => f.Channel!.ChannelName == request.ChannelName);

        if (!string.IsNullOrEmpty(request.ChannelType))
            query = query.Where(f => f.Channel!.ChannelType == request.ChannelType);

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

// Extension method para ordenamiento dinámico (reutilizable)
public static class ExportQueryableExtensions
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
