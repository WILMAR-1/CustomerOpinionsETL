using System.ComponentModel.DataAnnotations.Schema;

namespace CustomerOpinionsETL.Domain.Entities;

/// <summary>
/// Dimensión de Tiempo - Clave para analizar tendencias temporales
/// </summary>
public class DimDate
{
    [Column("date_key")]
    public int DateKey { get; set; }

    [Column("full_date")]
    public DateTime FullDate { get; set; }

    [Column("day_of_month")]
    public int DayOfMonth { get; set; }

    [Column("month_number")]
    public int MonthNumber { get; set; }

    [Column("month_name")]
    public string MonthName { get; set; } = string.Empty;

    [Column("quarter")]
    public int Quarter { get; set; }

    [Column("year")]
    public int Year { get; set; }
}
