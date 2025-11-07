namespace CustomerOpinionsETL.Domain.Entities;

/// <summary>
/// Dimensión de Cliente - Almacena los atributos de los clientes que opinan
/// </summary>
public class DimCustomer
{
    public int CustomerKey { get; set; }
    public int? SourceCustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? Segment { get; set; }
    public string? AgeRange { get; set; }
}
