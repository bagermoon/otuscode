using System;

using Microsoft.Extensions.Logging;

namespace RestoRate.ReviewService.Application.Handlers;

internal static partial class ReviewEventHandlersLogger
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Обработка события: Ревью добавлен (ID: {ReviewId}, Ресторан ID: {RestaurantId})")]
    internal static partial void LogReviewAdded(this ILogger logger, Guid reviewId, Guid restaurantId);
}
