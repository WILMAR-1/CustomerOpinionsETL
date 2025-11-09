namespace CustomerOpinionsETL.Application.Common.Models;

/// <summary>
/// Respuesta estándar para todos los servicios
/// Patrón consistente para éxito y errores
/// </summary>
public class ServiceResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public ApiError? Error { get; set; }
    public string TraceId { get; set; } = Guid.NewGuid().ToString("N")[..16];

    // Success Result
    public static ServiceResponse<T> SuccessResult(T data, string? message = null)
    {
        return new ServiceResponse<T>
        {
            Success = true,
            Data = data,
            Message = message ?? "Operación completada exitosamente",
            Error = null
        };
    }

    // Failure Result
    public static ServiceResponse<T> FailureResult(
        string message,
        string errorType = "BUSINESS_ERROR",
        string? field = null,
        string? errorCode = null)
    {
        return new ServiceResponse<T>
        {
            Success = false,
            Data = default,
            Message = message,
            Error = new ApiError
            {
                Type = errorType,
                Code = errorCode ?? "OPINIONS-400001",
                Message = message,
                Details = string.IsNullOrEmpty(field)
                    ? new List<ErrorDetail>()
                    : new List<ErrorDetail>
                    {
                        new ErrorDetail
                        {
                            Field = field,
                            Message = message,
                            Code = errorCode ?? "OPINIONS-400001"
                        }
                    },
                SourceService = "opinions-etl-service"
            }
        };
    }

    public static ServiceResponse<T> ValidationFailureResult(List<ErrorDetail> validationErrors)
    {
        return new ServiceResponse<T>
        {
            Success = false,
            Data = default,
            Message = "Errores de validación encontrados",
            Error = new ApiError
            {
                Type = "VALIDATION_ERROR",
                Code = "OPINIONS-400001",
                Message = "La validación falló",
                Details = validationErrors,
                SourceService = "opinions-etl-service"
            }
        };
    }
}