using CustomerOpinionsETL.Application.Configuration;
using CustomerOpinionsETL.Application.Services;
using CustomerOpinionsETL.Domain.Interfaces;
using CustomerOpinionsETL.Infrastructure.Configuration;
using CustomerOpinionsETL.Infrastructure.Data;
using CustomerOpinionsETL.Infrastructure.Extractors;
using CustomerOpinionsETL.Infrastructure.Persistence;
using CustomerOpinionsETL.Worker;
using Microsoft.EntityFrameworkCore;
using Serilog;

// =====================================================
// CONFIGURACIÓN DE SERILOG
// =====================================================
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/etl-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}",
        retainedFileCountLimit: 30)
    .CreateLogger();

try
{
    Log.Information("========================================");
    Log.Information("Iniciando Customer Opinions ETL Worker");
    Log.Information("========================================");

    var builder = Host.CreateApplicationBuilder(args);

    // =====================================================
    // CONFIGURACIÓN DE LOGGING
    // =====================================================
    builder.Services.AddSerilog();

    // =====================================================
    // CONFIGURACIÓN DE OPCIONES (Options Pattern)
    // =====================================================
    builder.Services.Configure<CsvExtractorOptions>(
        builder.Configuration.GetSection(CsvExtractorOptions.SectionName));

    builder.Services.Configure<DatabaseExtractorOptions>(
        builder.Configuration.GetSection(DatabaseExtractorOptions.SectionName));

    builder.Services.Configure<ApiExtractorOptions>(
        builder.Configuration.GetSection(ApiExtractorOptions.SectionName));

    builder.Services.Configure<EtlOptions>(
        builder.Configuration.GetSection(EtlOptions.SectionName));

    // =====================================================
    // CONFIGURACIÓN DE ENTITY FRAMEWORK CORE
    // =====================================================
    builder.Services.AddDbContext<DwOpinionsContext>(options =>
    {
        var connectionString = builder.Configuration.GetConnectionString("DwOpinions");
        options.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
            sqlOptions.CommandTimeout(300); // 5 minutos
        });
    });

    // =====================================================
    // CONFIGURACIÓN DE HTTP CLIENT (para API Extractor)
    // =====================================================
    builder.Services.AddHttpClient();

    // =====================================================
    // INYECCIÓN DE DEPENDENCIAS - EXTRACTORES
    // =====================================================
    builder.Services.AddScoped<IExtractor, CsvExtractor>();
    builder.Services.AddScoped<IExtractor, DatabaseExtractor>();
    builder.Services.AddScoped<IExtractor, ApiExtractor>();

    // =====================================================
    // INYECCIÓN DE DEPENDENCIAS - SERVICIOS
    // =====================================================
    builder.Services.AddScoped<IDataLoader, DataLoader>();
    builder.Services.AddScoped<EtlService>();

    // =====================================================
    // WORKER SERVICE
    // =====================================================
    builder.Services.AddHostedService<Worker>();

    // =====================================================
    // BUILD Y RUN
    // =====================================================
    var host = builder.Build();

    // Verificar conectividad a la base de datos al inicio
    using (var scope = host.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<DwOpinionsContext>();
        Log.Information("Verificando conexión a la base de datos...");

        if (await context.Database.CanConnectAsync())
        {
            Log.Information("Conexión a la base de datos exitosa");
        }
        else
        {
            Log.Error("No se pudo conectar a la base de datos");
            throw new Exception("No se pudo establecer conexión con la base de datos analítica");
        }
    }

    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicación terminó inesperadamente");
    return 1;
}
finally
{
    Log.Information("========================================");
    Log.Information("Customer Opinions ETL Worker detenido");
    Log.Information("========================================");
    await Log.CloseAndFlushAsync();
}

return 0;
