using System.ComponentModel.DataAnnotations.Schema;

namespace CustomerOpinionsETL.Domain.Entities;

/// <summary>
/// Tabla de Hechos - Contiene las métricas y las claves foráneas a las dimensiones
/// Cada fila representa una opinión individual
/// </summary>
public class FactOpinion
{
    [Column("product_key")]
    public int ProductKey { get; set; }

    [Column("customer_key")]
    public int CustomerKey { get; set; }

    [Column("date_key")]
    public int DateKey { get; set; }

    [Column("channel_key")]
    public int ChannelKey { get; set; }

    [Column("rating")]
    public int? Rating { get; set; }

    [Column("sentiment_score")]
    public int SentimentScore { get; set; }

    [Column("opinion_count")]
    public int OpinionCount { get; set; } = 1;

    // Navigation properties
    public DimProduct? Product { get; set; }
    public DimCustomer? Customer { get; set; }
    public DimDate? Date { get; set; }
    public DimChannel? Channel { get; set; }
}
