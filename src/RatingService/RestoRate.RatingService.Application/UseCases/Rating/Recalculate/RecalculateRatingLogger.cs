using System;

using Microsoft.Extensions.Logging;

namespace RestoRate.RatingService.Application.UseCases.Rating.Recalculate;

internal static partial class RecalculateRatingLogger
{
    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to recalculate rating for restaurant {RestaurantId}.")]
    internal static partial void LogRecalculationFailed(this ILogger logger, Guid restaurantId, Exception ex);

}
