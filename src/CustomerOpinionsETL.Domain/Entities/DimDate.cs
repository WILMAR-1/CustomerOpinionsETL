namespace CustomerOpinionsETL.Domain.Entities;

/// <summary>
/// Dimensión de Tiempo - Clave para analizar tendencias temporales
/// </summary>
public class DimDate
{
    public int DateKey { get; set; }
    public DateTime FullDate { get; set; }
    public int DayOfMonth { get; set; }
    public int MonthNumber { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public int Quarter { get; set; }
    public int Year { get; set; }
}
