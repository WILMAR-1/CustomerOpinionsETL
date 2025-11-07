using CustomerOpinionsETL.Domain.Entities;

namespace CustomerOpinionsETL.Domain.Interfaces;

/// <summary>
/// Interfaz base para todos los extractores de datos
/// Abstrae el origen de los datos (CSV, Database, API)
/// </summary>
public interface IExtractor
{
    /// <summary>
    /// Extrae datos de la fuente y los retorna como lista de OpinionDto
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación para operaciones asíncronas</param>
    /// <returns>Lista de opiniones extraídas</returns>
    Task<IEnumerable<OpinionDto>> ExtractAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Nombre del extractor para logging
    /// </summary>
    string ExtractorName { get; }
}
