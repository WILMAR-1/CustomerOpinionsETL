namespace CustomerOpinionsETL.Domain.Entities;

/// <summary>
/// Dimensión de Producto - Almacena información de los productos
/// </summary>
public class DimProduct
{
    public int ProductKey { get; set; }
    public int? SourceProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductCategory { get; set; }
    public string? ProductBrand { get; set; }
    public DateTime? LaunchDate { get; set; }
}
