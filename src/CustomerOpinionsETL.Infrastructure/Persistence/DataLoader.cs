using System.Collections.Concurrent;
using CustomerOpinionsETL.Application.Configuration;
using CustomerOpinionsETL.Domain.Entities;
using CustomerOpinionsETL.Domain.Interfaces;
using CustomerOpinionsETL.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CustomerOpinionsETL.Infrastructure.Persistence;

/// <summary>
/// Implementación del DataLoader para cargar datos en la base de datos analítica
/// Optimizado para alto rendimiento con procesamiento por lotes y caché de dimensiones
/// </summary>
public class DataLoader : IDataLoader
{
    private readonly DwOpinionsContext _context;
    private readonly ILogger<DataLoader> _logger;
    private readonly EtlOptions _options;

    // Cachés para lookups de dimensiones (optimización de rendimiento)
    private readonly ConcurrentDictionary<string, int> _productCache = new();
    private readonly ConcurrentDictionary<string, int> _customerCache = new();
    private readonly ConcurrentDictionary<int, int> _dateCache = new();
    private readonly ConcurrentDictionary<string, int> _channelCache = new();

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
        _logger.LogInformation("Iniciando carga de datos a la base de datos analítica");
        var startTime = DateTime.UtcNow;

        try
        {
            var opinionsList = opinions.ToList();
            _logger.LogInformation("Total de opiniones a cargar: {Count}", opinionsList.Count);

            // Prellenar cachés de dimensiones existentes
            await PreloadDimensionCachesAsync(cancellationToken);

            var totalInserted = 0;

            // Procesar en lotes para optimizar rendimiento
            var batches = opinionsList
                .Select((opinion, index) => new { opinion, index })
                .GroupBy(x => x.index / _options.BatchSize)
                .Select(g => g.Select(x => x.opinion).ToList());

            foreach (var batch in batches)
            {
                var inserted = await ProcessBatchAsync(batch, cancellationToken);
                totalInserted += inserted;

                _logger.LogInformation(
                    "Lote procesado: {Inserted} registros. Total acumulado: {Total}",
                    inserted, totalInserted);
            }

            var elapsed = DateTime.UtcNow - startTime;
            _logger.LogInformation(
                "Carga completada. {Total} registros insertados en {ElapsedMs}ms. " +
                "Rendimiento: {RecordsPerSecond} registros/segundo",
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

    private async Task<int> ProcessBatchAsync(
        List<OpinionDto> batch,
        CancellationToken cancellationToken)
    {
        var factOpinions = new List<FactOpinion>();

        foreach (var opinion in batch)
        {
            // Obtener o crear las claves de las dimensiones
            var productKey = await GetOrCreateProductKeyAsync(opinion, cancellationToken);
            var customerKey = await GetOrCreateCustomerKeyAsync(opinion, cancellationToken);
            var dateKey = await GetOrCreateDateKeyAsync(opinion.OpinionDate, cancellationToken);
            var channelKey = await GetOrCreateChannelKeyAsync(opinion, cancellationToken);

            // Crear el registro de hecho
            var factOpinion = new FactOpinion
            {
                ProductKey = productKey,
                CustomerKey = customerKey,
                DateKey = dateKey,
                ChannelKey = channelKey,
                Rating = opinion.Rating,
                SentimentScore = opinion.SentimentScore,
                OpinionCount = 1
            };

            factOpinions.Add(factOpinion);
        }

        // Bulk insert de hechos
        await _context.FactOpinions.AddRangeAsync(factOpinions, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return factOpinions.Count;
    }

    private async Task PreloadDimensionCachesAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Precargando cachés de dimensiones...");

        // Cargar productos
        var products = await _context.DimProducts
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        foreach (var product in products)
        {
            var key = $"{product.SourceProductId}_{product.ProductName}";
            _productCache.TryAdd(key, product.ProductKey);
        }

        // Cargar clientes
        var customers = await _context.DimCustomers
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        foreach (var customer in customers)
        {
            var key = $"{customer.SourceCustomerId}_{customer.CustomerName}";
            _customerCache.TryAdd(key, customer.CustomerKey);
        }

        // Cargar fechas
        var dates = await _context.DimDates
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        foreach (var date in dates)
        {
            _dateCache.TryAdd(date.DateKey, date.DateKey);
        }

        // Cargar canales
        var channels = await _context.DimChannels
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        foreach (var channel in channels)
        {
            _channelCache.TryAdd(channel.ChannelName, channel.ChannelKey);
        }

        _logger.LogInformation(
            "Cachés precargados: {Products} productos, {Customers} clientes, " +
            "{Dates} fechas, {Channels} canales",
            _productCache.Count, _customerCache.Count, _dateCache.Count, _channelCache.Count);
    }

    private async Task<int> GetOrCreateProductKeyAsync(
        OpinionDto opinion,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"{opinion.SourceProductId}_{opinion.ProductName}";

        if (_productCache.TryGetValue(cacheKey, out var existingKey))
        {
            return existingKey;
        }

        // Buscar en la base de datos
        var existing = await _context.DimProducts
            .FirstOrDefaultAsync(
                p => p.SourceProductId == opinion.SourceProductId &&
                     p.ProductName == opinion.ProductName,
                cancellationToken);

        if (existing != null)
        {
            _productCache.TryAdd(cacheKey, existing.ProductKey);
            return existing.ProductKey;
        }

        // Crear nuevo producto
        var newProduct = new DimProduct
        {
            SourceProductId = opinion.SourceProductId,
            ProductName = opinion.ProductName,
            ProductCategory = opinion.ProductCategory,
            ProductBrand = opinion.ProductBrand
        };

        _context.DimProducts.Add(newProduct);
        await _context.SaveChangesAsync(cancellationToken);

        _productCache.TryAdd(cacheKey, newProduct.ProductKey);
        return newProduct.ProductKey;
    }

    private async Task<int> GetOrCreateCustomerKeyAsync(
        OpinionDto opinion,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"{opinion.SourceCustomerId}_{opinion.CustomerName}";

        if (_customerCache.TryGetValue(cacheKey, out var existingKey))
        {
            return existingKey;
        }

        var existing = await _context.DimCustomers
            .FirstOrDefaultAsync(
                c => c.SourceCustomerId == opinion.SourceCustomerId &&
                     c.CustomerName == opinion.CustomerName,
                cancellationToken);

        if (existing != null)
        {
            _customerCache.TryAdd(cacheKey, existing.CustomerKey);
            return existing.CustomerKey;
        }

        var newCustomer = new DimCustomer
        {
            SourceCustomerId = opinion.SourceCustomerId,
            CustomerName = opinion.CustomerName,
            Country = opinion.Country,
            City = opinion.City,
            Segment = opinion.Segment,
            AgeRange = opinion.AgeRange
        };

        _context.DimCustomers.Add(newCustomer);
        await _context.SaveChangesAsync(cancellationToken);

        _customerCache.TryAdd(cacheKey, newCustomer.CustomerKey);
        return newCustomer.CustomerKey;
    }

    private async Task<int> GetOrCreateDateKeyAsync(
        DateTime date,
        CancellationToken cancellationToken)
    {
        var dateKey = int.Parse(date.ToString("yyyyMMdd"));

        if (_dateCache.ContainsKey(dateKey))
        {
            return dateKey;
        }

        var existing = await _context.DimDates
            .FindAsync(new object[] { dateKey }, cancellationToken);

        if (existing != null)
        {
            _dateCache.TryAdd(dateKey, dateKey);
            return dateKey;
        }

        var newDate = new DimDate
        {
            DateKey = dateKey,
            FullDate = date.Date,
            DayOfMonth = date.Day,
            MonthNumber = date.Month,
            MonthName = date.ToString("MMMM"),
            Quarter = (date.Month - 1) / 3 + 1,
            Year = date.Year
        };

        _context.DimDates.Add(newDate);
        await _context.SaveChangesAsync(cancellationToken);

        _dateCache.TryAdd(dateKey, dateKey);
        return dateKey;
    }

    private async Task<int> GetOrCreateChannelKeyAsync(
        OpinionDto opinion,
        CancellationToken cancellationToken)
    {
        if (_channelCache.TryGetValue(opinion.ChannelName, out var existingKey))
        {
            return existingKey;
        }

        var existing = await _context.DimChannels
            .FirstOrDefaultAsync(
                c => c.ChannelName == opinion.ChannelName,
                cancellationToken);

        if (existing != null)
        {
            _channelCache.TryAdd(opinion.ChannelName, existing.ChannelKey);
            return existing.ChannelKey;
        }

        var newChannel = new DimChannel
        {
            ChannelName = opinion.ChannelName,
            ChannelType = opinion.ChannelType
        };

        _context.DimChannels.Add(newChannel);
        await _context.SaveChangesAsync(cancellationToken);

        _channelCache.TryAdd(opinion.ChannelName, newChannel.ChannelKey);
        return newChannel.ChannelKey;
    }
}
