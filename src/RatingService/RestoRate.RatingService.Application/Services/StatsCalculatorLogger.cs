using Microsoft.Extensions.Logging;

namespace RestoRate.RatingService.Application.Services;

internal static partial class StatsCalculatorLogger
{
    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to debounce rating recalculation for restaurant {RestaurantId}")]
    internal static partial void FailedToDebounce(this ILogger logger, Exception ex, Guid restaurantId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to read rating cache for restaurant {RestaurantId}")]
    internal static partial void FailedToReadCache(this ILogger logger, Exception ex, Guid restaurantId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to update rating cache for restaurant {RestaurantId}")]
    internal static partial void FailedToUpdateCache(this ILogger logger, Exception ex, Guid restaurantId);
}
