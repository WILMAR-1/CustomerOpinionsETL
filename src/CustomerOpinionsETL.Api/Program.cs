using CustomerOpinionsETL.Application;
using CustomerOpinionsETL.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// CONFIGURACIÓN DE SERVICIOS
// ============================================

// Capas de la aplicación
builder.Services.AddApplication();  // MediatR, FluentValidation, Behaviors
builder.Services.AddInfrastructure(builder.Configuration);  // DbContext, Repositories

// Controllers y API
builder.Services.AddControllers();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Customer Opinions ETL API",
        Version = "v1",
        Description = "API para consultas analíticas del Data Warehouse de opiniones de clientes. Optimizada para consultas de 500K+ registros en menos de 5 segundos.",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "ETL Team"
        }
    });
});

// CORS (si se necesita para frontend)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ============================================
// CONFIGURACIÓN DEL PIPELINE
// ============================================

var app = builder.Build();

// Swagger en Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Customer Opinions ETL API v1");
        c.RoutePrefix = string.Empty;  // Swagger en la raíz
    });
}

// Middleware de excepción global
app.UseMiddleware<CustomerOpinionsETL.Api.Middleware.GlobalExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
