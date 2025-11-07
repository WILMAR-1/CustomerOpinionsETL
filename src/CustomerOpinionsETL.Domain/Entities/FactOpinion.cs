namespace CustomerOpinionsETL.Domain.Entities;

/// <summary>
/// Tabla de Hechos - Contiene las métricas y las claves foráneas a las dimensiones
/// Cada fila representa una opinión individual
/// </summary>
public class FactOpinion
{
    public int ProductKey { get; set; }
    public int CustomerKey { get; set; }
    public int DateKey { get; set; }
    public int ChannelKey { get; set; }
    public int? Rating { get; set; }
    public int SentimentScore { get; set; }
    public int OpinionCount { get; set; } = 1;

    // Navigation properties
    public DimProduct? Product { get; set; }
    public DimCustomer? Customer { get; set; }
    public DimDate? Date { get; set; }
    public DimChannel? Channel { get; set; }
}
