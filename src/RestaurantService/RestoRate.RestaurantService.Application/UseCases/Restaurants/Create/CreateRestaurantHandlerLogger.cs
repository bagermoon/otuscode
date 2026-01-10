using Microsoft.Extensions.Logging;

namespace RestoRate.RestaurantService.Application.UseCases.Restaurants.Create;

internal static partial class CreateRestaurantHandlerLogger
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Обработка команды создания ресторана: {RestaurantName}")]
    internal static partial void LogCreating(this ILogger logger, string restaurantName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Не удалось создать ресторан")]
    internal static partial void LogCreateFailed(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Ресторан создан успешно: ID {RestaurantId}")]
    internal static partial void LogCreated(this ILogger logger, object restaurantId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Ошибка при создании ресторана")]
    internal static partial void LogCreateError(this ILogger logger, Exception ex);
}
