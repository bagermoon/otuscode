using Microsoft.Extensions.Logging;

namespace RestoRate.RestaurantService.Application.UseCases.Restaurants.GetById;

internal static partial class GetRestaurantByIdHandlerLogger
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Обработка запроса получения ресторана: ID {RestaurantId}")]
    internal static partial void LogGettingById(this ILogger logger, Guid restaurantId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Ресторан не найден: ID {RestaurantId}")]
    internal static partial void LogRestaurantNotFound(this ILogger logger, Guid restaurantId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Ресторан найден успешно: ID {RestaurantId}")]
    internal static partial void LogFound(this ILogger logger, Guid restaurantId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Ошибка при получении ресторана")]
    internal static partial void LogGetError(this ILogger logger, Exception ex);
}
