using Microsoft.Extensions.Logging;

namespace RestoRate.ReviewService.Application.UseCases.Reviews.Create;

internal static partial class CreateReviewHandlerLogger
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Обработка команды создания отзыва: RestaurantId {RestaurantId}, UserId {UserId}")]
    internal static partial void LogHandling(this ILogger logger, Guid restaurantId, Guid userId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Не удалось создать отзыв")]
    internal static partial void LogCreateFailed(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Отзыв создан успешно: ID {ReviewId}")]
    internal static partial void LogCreated(this ILogger logger, object reviewId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Ошибка при создании отзыва")]
    internal static partial void LogCreateError(this ILogger logger, Exception ex);
}
