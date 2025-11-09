using System.Reflection;
using CustomerOpinionsETL.Application.Common.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CustomerOpinionsETL.Application;

/// <summary>
/// Configuración de Dependency Injection para la capa de Application
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // MediatR - CQRS Pattern
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        // FluentValidation - Validadores automáticos
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Pipeline Behaviors
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

        return services;
    }
}
