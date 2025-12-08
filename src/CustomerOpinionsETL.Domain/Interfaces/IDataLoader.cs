using CustomerOpinionsETL.Domain.Entities;

namespace CustomerOpinionsETL.Domain.Interfaces;

/// <summary>
/// Interfaz para cargar datos procesados en la base de datos analítica
/// </summary>
public interface IDataLoader
{
    /// <summary>
    /// Carga un lote de opiniones en la base de datos analítica
    /// Implementa procesamiento por lotes (bulk insert) para alto rendimiento
    /// </summary>
    /// <param name="opinions">Opiniones a cargar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Número de registros insertados</returns>
    Task<int> LoadOpinionsAsync(IEnumerable<OpinionDto> opinions, CancellationToken cancellationToken = default);

    /// <summary>
    /// Limpia (trunca) la tabla de hechos antes de una nueva carga
    /// IMPORTANTE: Esta operación elimina TODOS los registros de la tabla Fact_Opinions
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Número de registros eliminados</returns>
    Task<int> TruncateFactTableAsync(CancellationToken cancellationToken = default);
}
