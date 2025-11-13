using Microsoft.Extensions.Logging;

namespace RestoRate.Restaurant.Application.Handlers;

internal static partial class RestaurantEventHandlersLogger
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Обработка события: Ресторан '{RestaurantName}' создан (ID: {RestaurantId})")]
    internal static partial void LogRestaurantCreated(this ILogger logger, string restaurantName, int restaurantId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Обработка события: Ресторан '{RestaurantName}' обновлен (ID: {RestaurantId})")]
    internal static partial void LogRestaurantUpdated(this ILogger logger, string restaurantName, int restaurantId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Обработка события: Ресторан удален (ID: {RestaurantId})")]
    internal static partial void LogRestaurantDeleted(this ILogger logger, int restaurantId);
}
