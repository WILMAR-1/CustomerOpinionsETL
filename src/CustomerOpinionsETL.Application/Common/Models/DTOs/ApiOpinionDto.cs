namespace CustomerOpinionsETL.Application.Common.Models.DTOs;

/// <summary>
/// DTO para mapear la respuesta de la API
/// Ajustar según la estructura real del API
/// </summary>
public class ApiOpinionDto
{
    public int ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? Category { get; set; }
    public string? Brand { get; set; }

    public int UserId { get; set; }
    public string? UserName { get; set; }
    public LocationDto? Location { get; set; }
    public string? UserSegment { get; set; }
    public string? AgeRange { get; set; }

    public DateTime CommentDate { get; set; }
    public string? Source { get; set; }
    public int? Rating { get; set; }
    public int SentimentScore { get; set; }
    public string? CommentText { get; set; }
}
