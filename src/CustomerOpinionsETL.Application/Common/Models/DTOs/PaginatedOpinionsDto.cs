namespace CustomerOpinionsETL.Application.Common.Models.DTOs;

/// <summary>
/// DTO de respuesta paginada para opiniones
/// Incluye metadata de paginación para facilitar navegación en el cliente
/// </summary>
public class PaginatedOpinionsDto
{
    public List<OpinionReadDto> Opinions { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
    public double QueryTimeMs { get; set; }  // Para monitoreo de performance
}
