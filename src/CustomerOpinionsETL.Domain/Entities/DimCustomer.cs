using System.ComponentModel.DataAnnotations.Schema;

namespace CustomerOpinionsETL.Domain.Entities;

/// <summary>
/// Dimensión de Cliente - Almacena los atributos de los clientes que opinan
/// </summary>
public class DimCustomer
{
    [Column("customer_key")]
    public int CustomerKey { get; set; }

    [Column("source_customer_id")]
    public int? SourceCustomerId { get; set; }

    [Column("customer_name")]
    public string? CustomerName { get; set; }

    [Column("country")]
    public string? Country { get; set; }

    [Column("city")]
    public string? City { get; set; }

    [Column("segment")]
    public string? Segment { get; set; }

    [Column("age_range")]
    public string? AgeRange { get; set; }
}
