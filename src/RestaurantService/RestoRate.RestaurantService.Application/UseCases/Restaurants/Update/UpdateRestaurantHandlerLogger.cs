using Microsoft.Extensions.Logging;

namespace RestoRate.RestaurantService.Application.UseCases.Restaurants.Update;

internal static partial class UpdateRestaurantHandlerLogger
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Обработка команды обновления ресторана: ID {RestaurantId}")]
    internal static partial void LogUpdating(this ILogger logger, Guid restaurantId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Ресторан не найден: ID {RestaurantId}")]
    internal static partial void LogNotFound(this ILogger logger, Guid restaurantId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Не удалось обновить ресторан")]
    internal static partial void LogUpdateFailed(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Ресторан обновлен успешно: ID {RestaurantId}")]
    internal static partial void LogUpdated(this ILogger logger, Guid restaurantId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Ошибка при обновлении ресторана")]
    internal static partial void LogUpdateError(this ILogger logger, Exception ex);
}
