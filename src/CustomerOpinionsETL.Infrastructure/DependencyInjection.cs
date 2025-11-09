using CustomerOpinionsETL.Application.Common.Interfaces;
using CustomerOpinionsETL.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CustomerOpinionsETL.Infrastructure;

/// <summary>
/// Configuración de Dependency Injection para la capa de Infrastructure
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext - DW_Opiniones
        services.AddDbContext<DwOpinionsContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DwOpinions"),
                b => b.MigrationsAssembly(typeof(DwOpinionsContext).Assembly.FullName)));

        // Registrar DbContext como IApplicationDbContext para Clean Architecture
        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<DwOpinionsContext>());

        // HttpClient para extracción de APIs externas
        services.AddHttpClient();

        return services;
    }
}
