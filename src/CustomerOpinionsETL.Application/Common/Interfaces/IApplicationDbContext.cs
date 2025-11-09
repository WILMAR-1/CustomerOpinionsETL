using CustomerOpinionsETL.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CustomerOpinionsETL.Application.Common.Interfaces;

/// <summary>
/// Interfaz para el contexto de base de datos
/// Permite que Application defina sus necesidades sin depender de Infrastructure
/// </summary>
public interface IApplicationDbContext
{
    // DbSets principales
    DbSet<FactOpinion> FactOpinions { get; set; }
    DbSet<DimProduct> DimProducts { get; set; }
    DbSet<DimCustomer> DimCustomers { get; set; }
    DbSet<DimDate> DimDates { get; set; }
    DbSet<DimChannel> DimChannels { get; set; }

    // Métodos de EF Core necesarios para queries
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
