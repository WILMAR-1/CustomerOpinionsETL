using System.Net.Http.Json;
using System.Text.Json;
using CustomerOpinionsETL.Application.Models.Dto;
using CustomerOpinionsETL.Domain.Entities;
using CustomerOpinionsETL.Domain.Interfaces;
using CustomerOpinionsETL.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CustomerOpinionsETL.Infrastructure.Extractors;

/// <summary>
/// Extractor de datos desde API REST
/// Usa IHttpClientFactory para manejo eficiente de conexiones
/// Implementa retry logic para resiliencia
/// </summary>
public class ApiExtractor : IExtractor
{
    private readonly ApiExtractorOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ApiExtractor> _logger;

    public string ExtractorName => "API Extractor";

    public ApiExtractor(
        IOptions<ApiExtractorOptions> options,
        IHttpClientFactory httpClientFactory,
        ILogger<ApiExtractor> logger)
    {
        _options = options.Value;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<IEnumerable<OpinionDto>> ExtractAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Iniciando extracción de API: {BaseUrl}{Endpoint}",
            _options.BaseUrl, _options.Endpoint);

        var startTime = DateTime.UtcNow;

        try
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);

            // Agregar API Key si está configurada
            if (!string.IsNullOrEmpty(_options.ApiKey))
            {
                client.DefaultRequestHeaders.Add("X-API-Key", _options.ApiKey);
            }

            // Implementar retry logic
            var opinions = await ExecuteWithRetryAsync(
                async () => await FetchOpinionsFromApiAsync(client, cancellationToken),
                _options.MaxRetries,
                cancellationToken);

            var elapsed = DateTime.UtcNow - startTime;
            _logger.LogInformation(
                "Extracción de API completada. {Count} registros extraídos en {ElapsedMs}ms",
                opinions.Count(), elapsed.TotalMilliseconds);

            return opinions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al extraer datos de la API");
            throw;
        }
    }

    private async Task<IEnumerable<OpinionDto>> FetchOpinionsFromApiAsync(
        HttpClient client,
        CancellationToken cancellationToken)
    {
        var response = await client.GetAsync(_options.Endpoint, cancellationToken);
        response.EnsureSuccessStatusCode();

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        // Asumiendo que el API retorna un array de opiniones
        var apiData = await response.Content.ReadFromJsonAsync<List<ApiOpinionDto>>(
            jsonOptions,
            cancellationToken);

        if (apiData == null)
        {
            _logger.LogWarning("La API retornó datos nulos");
            return Enumerable.Empty<OpinionDto>();
        }

        return apiData.Select(MapToOpinionDto);
    }

    private async Task<T> ExecuteWithRetryAsync<T>(
        Func<Task<T>> action,
        int maxRetries,
        CancellationToken cancellationToken)
    {
        var attempt = 0;

        while (true)
        {
            try
            {
                return await action();
            }
            catch (HttpRequestException ex) when (attempt < maxRetries)
            {
                attempt++;
                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt)); // Exponential backoff

                _logger.LogWarning(ex,
                    "Error en llamada a API. Intento {Attempt} de {MaxRetries}. Reintentando en {Delay}s",
                    attempt, maxRetries, delay.TotalSeconds);

                await Task.Delay(delay, cancellationToken);
            }
        }
    }

    private OpinionDto MapToOpinionDto(ApiOpinionDto apiDto)
    {
        return new OpinionDto
        {
            SourceProductId = apiDto.ProductId,
            ProductName = apiDto.ProductName ?? "Unknown",
            ProductCategory = apiDto.Category,
            ProductBrand = apiDto.Brand,

            SourceCustomerId = apiDto.UserId,
            CustomerName = apiDto.UserName ?? "Anonymous",
            Country = apiDto.Location?.Country,
            City = apiDto.Location?.City,
            Segment = apiDto.UserSegment,
            AgeRange = apiDto.AgeRange,

            OpinionDate = apiDto.CommentDate,
            ChannelName = apiDto.Source ?? "Social Media",
            ChannelType = "API",

            Rating = apiDto.Rating,
            SentimentScore = apiDto.SentimentScore,
            CommentText = apiDto.CommentText
        };
    }
}
