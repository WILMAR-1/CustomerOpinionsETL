namespace CustomerOpinionsETL.Application.Common.Constants;

/// <summary>
/// Códigos de error estandarizados
/// Formato: OPINIONS-{HTTP_STATUS}{SPECIFIC_CODE}
/// </summary>
public static class ErrorCodes
{
    // 400 Bad Request Errors
    public const string ValidationFailed = "OPINIONS-400001";
    public const string InvalidPaginationParameters = "OPINIONS-400002";
    public const string InvalidDateRange = "OPINIONS-400003";
    public const string InvalidFilterCombination = "OPINIONS-400004";

    // 404 Not Found
    public const string OpinionNotFound = "OPINIONS-404001";
    public const string ProductNotFound = "OPINIONS-404002";
    public const string CustomerNotFound = "OPINIONS-404003";

    // 409 Conflict
    public const string DuplicateEntry = "OPINIONS-409001";

    // 500 Internal Server Error
    public const string InternalServerError = "OPINIONS-500001";
    public const string DatabaseError = "OPINIONS-500002";
    public const string UnexpectedError = "OPINIONS-500003";

    // 503 Service Unavailable
    public const string DatabaseUnavailable = "OPINIONS-503001";
}
