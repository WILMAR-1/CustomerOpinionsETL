using System.ComponentModel.DataAnnotations.Schema;

namespace CustomerOpinionsETL.Domain.Entities;

/// <summary>
/// Dimensión de Producto - Almacena información de los productos
/// </summary>
public class DimProduct
{
    [Column("product_key")]
    public int ProductKey { get; set; }

    [Column("source_product_id")]
    public int? SourceProductId { get; set; }

    [Column("product_name")]
    public string? ProductName { get; set; }

    [Column("product_category")]
    public string? ProductCategory { get; set; }

    [Column("product_brand")]
    public string? ProductBrand { get; set; }

    [Column("launch_date")]
    public DateTime? LaunchDate { get; set; }
}
