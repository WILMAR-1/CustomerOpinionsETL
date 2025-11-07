namespace CustomerOpinionsETL.Domain.Entities;

/// <summary>
/// DTO específico para datos extraídos de CSV (Encuestas Internas)
/// </summary>
public class CsvOpinionDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public DateTime SurveyDate { get; set; }
    public int Rating { get; set; }
    public int Sentiment { get; set; }
    public string? Comment { get; set; }
}
