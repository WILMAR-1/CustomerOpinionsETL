namespace CustomerOpinionsETL.Domain.Entities;

/// <summary>
/// DTO genérico para representar una opinión extraída de cualquier fuente
/// </summary>
public class OpinionDto
{
    public int SourceProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductCategory { get; set; }
    public string? ProductBrand { get; set; }

    public int SourceCustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? Segment { get; set; }
    public string? AgeRange { get; set; }

    public DateTime OpinionDate { get; set; }
    public string ChannelName { get; set; } = string.Empty;
    public string ChannelType { get; set; } = string.Empty;

    public int? Rating { get; set; }
    public int SentimentScore { get; set; }
    public string? CommentText { get; set; }
}
