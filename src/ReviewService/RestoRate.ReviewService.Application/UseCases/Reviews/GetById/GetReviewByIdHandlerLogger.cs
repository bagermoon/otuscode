using Microsoft.Extensions.Logging;

namespace RestoRate.ReviewService.Application.UseCases.Reviews.GetById;

internal static partial class GetReviewByIdHandlerLogger
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Получение отзыва по Id: {ReviewId}")]
    internal static partial void LogGetting(this ILogger logger, Guid reviewId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Отзыв не найден: {ReviewId}")]
    internal static partial void LogNotFound(this ILogger logger, Guid reviewId);
}
