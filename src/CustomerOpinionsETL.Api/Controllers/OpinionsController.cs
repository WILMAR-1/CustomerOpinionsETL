using CustomerOpinionsETL.Application.Common.Models;
using CustomerOpinionsETL.Application.Common.Models.DTOs;
using CustomerOpinionsETL.Application.Features.Opinions.Queries.SearchOpinions;
using CustomerOpinionsETL.Application.Features.Opinions.Queries.ExportOpinions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CustomerOpinionsETL.Api.Controllers;

/// <summary>
/// Controlador para gestión y consultas de opiniones del Data Warehouse
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OpinionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OpinionsController> _logger;

    public OpinionsController(
        IMediator mediator,
        ILogger<OpinionsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Buscar opiniones con filtros y paginación
    /// </summary>
    /// <remarks>
    /// Endpoint optimizado para consultas de 500K+ registros en menos de 5 segundos.
    ///
    /// **Modos de consulta:**
    /// 1. **Paginación tradicional:** Page=1&amp;PageSize=50
    /// 2. **Límite directo:** Limit=500000 (obtiene los primeros N registros, ignora Page y PageSize)
    ///
    /// Filtros disponibles:
    /// - Producto: ProductKey, ProductName, ProductCategory, ProductBrand
    /// - Cliente: CustomerKey, CustomerName, Country, City, Segment
    /// - Canal: ChannelKey, ChannelName
    /// - Fecha: DateFrom, DateTo, Year, Month, Quarter
    /// - Métricas: RatingMin, RatingMax, SentimentScoreMin, SentimentScoreMax
    ///
    /// Ejemplos:
    /// - GET /api/opinions/search?Page=1&amp;PageSize=50&amp;ProductCategory=Electronics&amp;RatingMin=4
    /// - GET /api/opinions/search?Limit=500000&amp;RatingMin=4
    /// </remarks>
    /// <param name="query">Parámetros de búsqueda y paginación</param>
    /// <returns>Lista paginada de opiniones con información desnormalizada</returns>
    /// <response code="200">Búsqueda exitosa - retorna lista de opiniones</response>
    /// <response code="400">Parámetros de búsqueda inválidos</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet("search")]
    [ProducesResponseType(typeof(ServiceResponse<PaginatedOpinionsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ServiceResponse<PaginatedOpinionsDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ServiceResponse<PaginatedOpinionsDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ServiceResponse<PaginatedOpinionsDto>>> Search(
        [FromQuery] SearchOpinionsQuery query)
    {
        _logger.LogInformation(
            "Búsqueda de opiniones solicitada. Page: {Page}, PageSize: {PageSize}",
            query.Page, query.PageSize);

        var result = await _mediator.Send(query);

        if (!result.Success)
        {
            _logger.LogWarning(
                "Búsqueda de opiniones falló. Error: {Error}",
                result.Error?.Message);
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Obtener estadísticas generales del Data Warehouse
    /// </summary>
    /// <returns>Estadísticas de conteo y métricas</returns>
    /// <response code="200">Estadísticas obtenidas exitosamente</response>
    [HttpGet("stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetStats()
    {
        // TODO: Implementar query para estadísticas generales
        return Ok(new
        {
            Message = "Stats endpoint - To be implemented",
            Success = true
        });
    }

    /// <summary>
    /// Obtener agregaciones por dimensión (producto, cliente, etc.)
    /// </summary>
    /// <param name="dimension">Dimensión para agrupar (product, customer, channel, date)</param>
    /// <returns>Datos agregados por la dimensión especificada</returns>
    /// <response code="200">Agregaciones obtenidas exitosamente</response>
    /// <response code="400">Dimensión inválida</response>
    [HttpGet("aggregations/{dimension}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> GetAggregations(string dimension)
    {
        // TODO: Implementar queries de agregación por dimensión
        return Ok(new
        {
            Dimension = dimension,
            Message = "Aggregations endpoint - To be implemented",
            Success = true
        });
    }

    /// <summary>
    /// Exportar opiniones del Data Warehouse a formato CSV
    /// </summary>
    /// <remarks>
    /// Proceso inverso al ETL: extrae datos del DW y los exporta a CSV.
    ///
    /// **Ejemplos de uso:**
    ///
    /// 1. Exportar todas las opiniones (sin límite):
    ///    POST /api/opinions/export
    ///    {}
    ///
    /// 2. Exportar primeras 10,000 opiniones:
    ///    POST /api/opinions/export
    ///    { "Limit": 10000 }
    ///
    /// 3. Exportar opiniones de un rango de fechas:
    ///    POST /api/opinions/export
    ///    {
    ///      "DateFrom": "2024-01-01",
    ///      "DateTo": "2024-12-31",
    ///      "Limit": 50000
    ///    }
    ///
    /// 4. Exportar con múltiples filtros:
    ///    POST /api/opinions/export
    ///    {
    ///      "DateFrom": "2024-01-01",
    ///      "DateTo": "2024-12-31",
    ///      "ProductCategory": "Electronics",
    ///      "RatingMin": 4,
    ///      "Country": "USA",
    ///      "Limit": 100000
    ///    }
    ///
    /// **Filtros disponibles:**
    /// - Fecha: DateFrom, DateTo
    /// - Producto: ProductName, ProductCategory, ProductBrand
    /// - Cliente: CustomerName, Country, City, Segment
    /// - Canal: ChannelName, ChannelType
    /// - Métricas: RatingMin, RatingMax, SentimentScoreMin, SentimentScoreMax
    /// - Límite: Limit (cantidad máxima de registros)
    /// - Ordenamiento: OrderBy, OrderDirection
    /// </remarks>
    /// <param name="query">Parámetros de exportación y filtros</param>
    /// <returns>Archivo CSV descargable con las opiniones</returns>
    /// <response code="200">Archivo CSV generado exitosamente</response>
    /// <response code="400">Parámetros de exportación inválidos o no se encontraron datos</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpPost("export")]
    [Produces("text/csv")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> ExportToCsv([FromBody] ExportOpinionsQuery query)
    {
        _logger.LogInformation(
            "Exportación de opiniones solicitada. Limit: {Limit}, DateFrom: {DateFrom}, DateTo: {DateTo}",
            query.Limit, query.DateFrom, query.DateTo);

        var result = await _mediator.Send(query);

        if (!result.Success)
        {
            _logger.LogWarning("Exportación falló. Error: {Error}", result.ErrorMessage);
            return BadRequest(new
            {
                Success = false,
                Message = result.ErrorMessage
            });
        }

        _logger.LogInformation(
            "Exportación completada. {Records} registros exportados en {TimeMs}ms",
            result.TotalRecords,
            result.ExportTimeMs);

        return File(
            result.CsvData,
            "text/csv",
            result.FileName);
    }
}
