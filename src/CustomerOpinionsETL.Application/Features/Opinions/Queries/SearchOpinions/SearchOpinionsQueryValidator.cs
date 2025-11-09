using FluentValidation;

namespace CustomerOpinionsETL.Application.Features.Opinions.Queries.SearchOpinions;

/// <summary>
/// Validator para SearchOpinionsQuery
/// Valida parámetros de paginación y filtros antes de ejecutar la query
/// </summary>
public class SearchOpinionsQueryValidator : AbstractValidator<SearchOpinionsQuery>
{
    public SearchOpinionsQueryValidator()
    {
        // Validación de Paginación (CRÍTICO para performance)
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page debe ser mayor que 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("PageSize debe ser mayor que 0");

        // Validación de Limit (alternativa a paginación)
        RuleFor(x => x.Limit)
            .GreaterThan(0)
            .When(x => x.Limit.HasValue)
            .WithMessage("Limit debe ser mayor que 0");

        // Validación de Fechas
        RuleFor(x => x.DateFrom)
            .LessThanOrEqualTo(x => x.DateTo)
            .When(x => x.DateFrom.HasValue && x.DateTo.HasValue)
            .WithMessage("DateFrom debe ser menor o igual que DateTo");

        RuleFor(x => x.Year)
            .InclusiveBetween(2000, 2100)
            .When(x => x.Year.HasValue)
            .WithMessage("Year debe estar entre 2000 y 2100");

        RuleFor(x => x.Month)
            .InclusiveBetween(1, 12)
            .When(x => x.Month.HasValue)
            .WithMessage("Month debe estar entre 1 y 12");

        RuleFor(x => x.Quarter)
            .InclusiveBetween(1, 4)
            .When(x => x.Quarter.HasValue)
            .WithMessage("Quarter debe estar entre 1 y 4");

        // Validación de Ratings
        RuleFor(x => x.RatingMin)
            .InclusiveBetween(1, 5)
            .When(x => x.RatingMin.HasValue)
            .WithMessage("RatingMin debe estar entre 1 y 5");

        RuleFor(x => x.RatingMax)
            .InclusiveBetween(1, 5)
            .When(x => x.RatingMax.HasValue)
            .WithMessage("RatingMax debe estar entre 1 y 5");

        RuleFor(x => x.RatingMin)
            .LessThanOrEqualTo(x => x.RatingMax)
            .When(x => x.RatingMin.HasValue && x.RatingMax.HasValue)
            .WithMessage("RatingMin debe ser menor o igual que RatingMax");

        // Validación de Sentiment Score
        RuleFor(x => x.SentimentScoreMin)
            .InclusiveBetween(-10, 10)
            .When(x => x.SentimentScoreMin.HasValue)
            .WithMessage("SentimentScoreMin debe estar entre -10 y 10");

        RuleFor(x => x.SentimentScoreMax)
            .InclusiveBetween(-10, 10)
            .When(x => x.SentimentScoreMax.HasValue)
            .WithMessage("SentimentScoreMax debe estar entre -10 y 10");

        RuleFor(x => x.SentimentScoreMin)
            .LessThanOrEqualTo(x => x.SentimentScoreMax)
            .When(x => x.SentimentScoreMin.HasValue && x.SentimentScoreMax.HasValue)
            .WithMessage("SentimentScoreMin debe ser menor o igual que SentimentScoreMax");

        // Validación de Ordenamiento
        RuleFor(x => x.OrderBy)
            .Must(BeValidOrderByField)
            .WithMessage("OrderBy debe ser uno de: DateKey, Rating, SentimentScore, ProductKey, CustomerKey");

        RuleFor(x => x.OrderDirection)
            .Must(BeValidOrderDirection)
            .WithMessage("OrderDirection debe ser 'asc' o 'desc'");
    }

    private static bool BeValidOrderByField(string orderBy)
    {
        var validFields = new[] { "DateKey", "Rating", "SentimentScore", "ProductKey", "CustomerKey" };
        return validFields.Contains(orderBy, StringComparer.OrdinalIgnoreCase);
    }

    private static bool BeValidOrderDirection(string direction)
    {
        return direction.Equals("asc", StringComparison.OrdinalIgnoreCase) ||
               direction.Equals("desc", StringComparison.OrdinalIgnoreCase);
    }
}
