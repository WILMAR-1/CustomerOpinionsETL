using CustomerOpinionsETL.Application.Common.Models;
using FluentValidation;
using MediatR;

namespace CustomerOpinionsETL.Application.Common.Behaviors;

/// <summary>
/// Pipeline Behavior para validación automática con FluentValidation
/// Se ejecuta ANTES del handler para validar el request
/// </summary>
public class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Si no hay validadores, continuar
        if (!_validators.Any())
        {
            return await next();
        }

        // Ejecutar todas las validaciones
        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        // Recolectar errores
        var failures = validationResults
            .Where(r => !r.IsValid)
            .SelectMany(r => r.Errors)
            .ToList();

        if (failures.Any())
        {
            // Si TResponse es ServiceResponse<T>, crear respuesta de error
            var responseType = typeof(TResponse);

            if (responseType.IsGenericType &&
                responseType.GetGenericTypeDefinition() == typeof(ServiceResponse<>))
            {
                var dataType = responseType.GetGenericArguments()[0];
                var serviceResponseType = typeof(ServiceResponse<>).MakeGenericType(dataType);

                // Convertir FluentValidation.Results a ErrorDetail
                var errorDetails = failures.Select(f => new ErrorDetail
                {
                    Field = f.PropertyName,
                    Message = f.ErrorMessage,
                    Code = f.ErrorCode ?? "VALIDATION_ERROR"
                }).ToList();

                // Crear ServiceResponse de error usando reflection
                var method = serviceResponseType.GetMethod("ValidationFailureResult");
                if (method != null)
                {
                    var result = method.Invoke(null, new object[] { errorDetails });
                    return (TResponse)result!;
                }
            }

            // Si no es ServiceResponse, lanzar excepción
            throw new ValidationException(failures);
        }

        // Continuar al siguiente behavior o al handler
        return await next();
    }
}
