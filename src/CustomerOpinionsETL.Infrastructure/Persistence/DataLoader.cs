using CustomerOpinionsETL.Application.Configuration;
using CustomerOpinionsETL.Domain.Entities;
using CustomerOpinionsETL.Domain.Interfaces;
using CustomerOpinionsETL.Infrastructure.Data;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CustomerOpinionsETL.Infrastructure.Persistence;

/// <summary>
/// Implementación del DataLoader con operaciones BULK puras sin bucles
/// Optimizado para máximo rendimiento con 500K+ registros
/// </summary>
public class DataLoader : IDataLoader
{
    private readonly DwOpinionsContext _context;
    private readonly ILogger<DataLoader> _logger;
    private readonly EtlOptions _options;

    public DataLoader(
        DwOpinionsContext context,
        ILogger<DataLoader> logger,
        IOptions<EtlOptions> options)
    {
        _context = context;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<int> LoadOpinionsAsync(
        IEnumerable<OpinionDto> opinions,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Iniciando carga BULK de datos a la base de datos analítica");
        var startTime = DateTime.UtcNow;

        try
        {
            var opinionsList = opinions.ToList();
            _logger.LogInformation("Total de opiniones a cargar: {Count}", opinionsList.Count);

            // PASO 1: Procesar dimensiones en BULK (sin bucles)
            await ProcessProductDimensionBulkAsync(opinionsList, cancellationToken);
            await ProcessCustomerDimensionBulkAsync(opinionsList, cancellationToken);
            await ProcessDateDimensionBulkAsync(opinionsList, cancellationToken);
            await ProcessChannelDimensionBulkAsync(opinionsList, cancellationToken);

            // PASO 2: Cargar cachés de dimensiones (sin bucles - usando ToDictionary)
            var productLookup = await LoadProductLookupAsync(cancellationToken);
            var customerLookup = await LoadCustomerLookupAsync(cancellationToken);
            var dateLookup = await LoadDateLookupAsync(cancellationToken);
            var channelLookup = await LoadChannelLookupAsync(cancellationToken);

            // PASO 3: Crear todos los registros de hechos en una sola operación (sin bucles)
            var factOpinions = opinionsList
                .Select(o => new FactOpinion
                {
                    ProductKey = productLookup[$"{o.SourceProductId}_{o.ProductName}"],
                    CustomerKey = customerLookup[$"{o.SourceCustomerId}_{o.CustomerName}"],
                    DateKey = dateLookup[int.Parse(o.OpinionDate.ToString("yyyyMMdd"))],
                    ChannelKey = channelLookup[o.ChannelName],
                    Rating = o.Rating,
                    SentimentScore = o.SentimentScore,
                    OpinionCount = 1
                })
                .ToList();

            // PASO 4: Bulk insert de todos los hechos de una vez
            await _context.BulkInsertAsync(factOpinions, cancellationToken: cancellationToken);

            var totalInserted = factOpinions.Count;
            var elapsed = DateTime.UtcNow - startTime;

            _logger.LogInformation(
                "Carga BULK completada. {Total} registros insertados en {ElapsedMs}ms. " +
                "Rendimiento: {RecordsPerSecond:F2} registros/segundo",
                totalInserted,
                elapsed.TotalMilliseconds,
                totalInserted / elapsed.TotalSeconds);

            return totalInserted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar datos en la base de datos analítica");
            throw;
        }
    }

    private async Task ProcessProductDimensionBulkAsync(
        List<OpinionDto> opinions,
        CancellationToken cancellationToken)
    {
        // Extraer productos únicos sin bucles
        var uniqueProducts = opinions
            .Select(o => new
            {
                o.SourceProductId,
                o.ProductName,
                o.ProductCategory,
                o.ProductBrand
            })
            .Distinct()
            .Select(p => new DimProduct
            {
                SourceProductId = p.SourceProductId,
                ProductName = p.ProductName,
                ProductCategory = p.ProductCategory,
                ProductBrand = p.ProductBrand
            })
            .ToList();

        // Bulk insert o update de todos los productos de una vez
        await _context.BulkInsertOrUpdateAsync(
            uniqueProducts,
            new BulkConfig
            {
                UpdateByProperties = new List<string> { nameof(DimProduct.SourceProductId), nameof(DimProduct.ProductName) },
                PropertiesToIncludeOnUpdate = new List<string> { nameof(DimProduct.ProductCategory), nameof(DimProduct.ProductBrand) }
            },
            cancellationToken: cancellationToken);

        _logger.LogInformation("Dimensión Productos procesada: {Count} registros únicos", uniqueProducts.Count);
    }

    private async Task ProcessCustomerDimensionBulkAsync(
        List<OpinionDto> opinions,
        CancellationToken cancellationToken)
    {
        // Extraer clientes únicos sin bucles
        var uniqueCustomers = opinions
            .Select(o => new
            {
                o.SourceCustomerId,
                o.CustomerName,
                o.Country,
                o.City,
                o.Segment,
                o.AgeRange
            })
            .Distinct()
            .Select(c => new DimCustomer
            {
                SourceCustomerId = c.SourceCustomerId,
                CustomerName = c.CustomerName,
                Country = c.Country,
                City = c.City,
                Segment = c.Segment,
                AgeRange = c.AgeRange
            })
            .ToList();

        // Bulk insert o update de todos los clientes de una vez
        await _context.BulkInsertOrUpdateAsync(
            uniqueCustomers,
            new BulkConfig
            {
                UpdateByProperties = new List<string> { nameof(DimCustomer.SourceCustomerId), nameof(DimCustomer.CustomerName) },
                PropertiesToIncludeOnUpdate = new List<string>
                {
                    nameof(DimCustomer.Country),
                    nameof(DimCustomer.City),
                    nameof(DimCustomer.Segment),
                    nameof(DimCustomer.AgeRange)
                }
            },
            cancellationToken: cancellationToken);

        _logger.LogInformation("Dimensión Clientes procesada: {Count} registros únicos", uniqueCustomers.Count);
    }

    private async Task ProcessDateDimensionBulkAsync(
        List<OpinionDto> opinions,
        CancellationToken cancellationToken)
    {
        // Extraer fechas únicas sin bucles
        var uniqueDates = opinions
            .Select(o => o.OpinionDate.Date)
            .Distinct()
            .Select(date => new DimDate
            {
                DateKey = int.Parse(date.ToString("yyyyMMdd")),
                FullDate = date,
                DayOfMonth = date.Day,
                MonthNumber = date.Month,
                MonthName = date.ToString("MMMM"),
                Quarter = (date.Month - 1) / 3 + 1,
                Year = date.Year
            })
            .ToList();

        // Bulk insert o update de todas las fechas de una vez
        await _context.BulkInsertOrUpdateAsync(
            uniqueDates,
            new BulkConfig
            {
                UpdateByProperties = new List<string> { nameof(DimDate.DateKey) }
            },
            cancellationToken: cancellationToken);

        _logger.LogInformation("Dimensión Fechas procesada: {Count} registros únicos", uniqueDates.Count);
    }

    private async Task ProcessChannelDimensionBulkAsync(
        List<OpinionDto> opinions,
        CancellationToken cancellationToken)
    {
        // Extraer canales únicos sin bucles
        var uniqueChannels = opinions
            .Select(o => new { o.ChannelName, o.ChannelType })
            .Distinct()
            .Select(c => new DimChannel
            {
                ChannelName = c.ChannelName,
                ChannelType = c.ChannelType
            })
            .ToList();

        // Bulk insert o update de todos los canales de una vez
        await _context.BulkInsertOrUpdateAsync(
            uniqueChannels,
            new BulkConfig
            {
                UpdateByProperties = new List<string> { nameof(DimChannel.ChannelName) },
                PropertiesToIncludeOnUpdate = new List<string> { nameof(DimChannel.ChannelType) }
            },
            cancellationToken: cancellationToken);

        _logger.LogInformation("Dimensión Canales procesada: {Count} registros únicos", uniqueChannels.Count);
    }

    private async Task<Dictionary<string, int>> LoadProductLookupAsync(CancellationToken cancellationToken)
    {
        // Cargar todos los productos y crear lookup en una sola operación (sin bucles)
        return await _context.DimProducts
            .AsNoTracking()
            .ToDictionaryAsync(
                p => $"{p.SourceProductId}_{p.ProductName}",
                p => p.ProductKey,
                cancellationToken);
    }

    private async Task<Dictionary<string, int>> LoadCustomerLookupAsync(CancellationToken cancellationToken)
    {
        // Cargar todos los clientes y crear lookup en una sola operación (sin bucles)
        return await _context.DimCustomers
            .AsNoTracking()
            .ToDictionaryAsync(
                c => $"{c.SourceCustomerId}_{c.CustomerName}",
                c => c.CustomerKey,
                cancellationToken);
    }

    private async Task<Dictionary<int, int>> LoadDateLookupAsync(CancellationToken cancellationToken)
    {
        // Cargar todas las fechas y crear lookup en una sola operación (sin bucles)
        return await _context.DimDates
            .AsNoTracking()
            .ToDictionaryAsync(
                d => d.DateKey,
                d => d.DateKey,
                cancellationToken);
    }

    private async Task<Dictionary<string, int>> LoadChannelLookupAsync(CancellationToken cancellationToken)
    {
        // Cargar todos los canales y crear lookup en una sola operación (sin bucles)
        return await _context.DimChannels
            .AsNoTracking()
            .ToDictionaryAsync(
                c => c.ChannelName,
                c => c.ChannelKey,
                cancellationToken);
    }
}
