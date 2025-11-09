using CustomerOpinionsETL.Application.Common.Constants;
using CustomerOpinionsETL.Application.Common.Models;
using System.Net;
using System.Text.Json;

namespace CustomerOpinionsETL.Api.Middleware;

/// <summary>
/// Middleware global para capturar y manejar excepciones de forma estandarizada
/// Convierte todas las excepciones en respuestas ServiceResponse consistentes
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excepción no manejada: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, errorResponse) = exception switch
        {
            // FluentValidation exceptions
            FluentValidation.ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                ServiceResponse<object>.ValidationFailureResult(
                    validationEx.Errors.Select(e => new ErrorDetail
                    {
                        Field = e.PropertyName,
                        Message = e.ErrorMessage,
                        Code = ErrorCodes.ValidationFailed
                    }).ToList()
                )
            ),

            // ArgumentException - Bad Request
            ArgumentException argEx => (
                HttpStatusCode.BadRequest,
                ServiceResponse<object>.FailureResult(
                    argEx.Message,
                    "BAD_REQUEST",
                    errorCode: ErrorCodes.ValidationFailed
                )
            ),

            // KeyNotFoundException - Not Found
            KeyNotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                ServiceResponse<object>.FailureResult(
                    notFoundEx.Message,
                    "NOT_FOUND",
                    errorCode: ErrorCodes.OpinionNotFound
                )
            ),

            // UnauthorizedAccessException - Unauthorized
            UnauthorizedAccessException unauthorizedEx => (
                HttpStatusCode.Unauthorized,
                ServiceResponse<object>.FailureResult(
                    "No autorizado para realizar esta operación",
                    "UNAUTHORIZED",
                    errorCode: "OPINIONS-401001"
                )
            ),

            // Default - Internal Server Error
            _ => (
                HttpStatusCode.InternalServerError,
                ServiceResponse<object>.FailureResult(
                    "Ocurrió un error interno en el servidor",
                    "INTERNAL_ERROR",
                    errorCode: ErrorCodes.InternalServerError
                )
            )
        };

        context.Response.StatusCode = (int)statusCode;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        var json = JsonSerializer.Serialize(errorResponse, jsonOptions);

        return context.Response.WriteAsync(json);
    }
}
