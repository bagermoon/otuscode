using Microsoft.Extensions.Logging;

namespace RestoRate.RestaurantService.Application.UseCases.RestaurantImage.SetPrimaryImage;

internal static partial class SetPrimaryImageHandlerLogger
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Установка главного изображения {ImageId} для ресторана {RestaurantId}")]
    internal static partial void LogSettingPrimary(this ILogger logger, Guid imageId, Guid restaurantId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Ресторан {RestaurantId} не найден")]
    internal static partial void LogRestaurantNotFound(this ILogger logger, Guid restaurantId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Изображение {ImageId} не найдено в ресторане {RestaurantId}")]
    internal static partial void LogImageNotFound(this ILogger logger, Guid imageId, Guid restaurantId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Изображение {ImageId} установлено как главное для ресторана {RestaurantId}")]
    internal static partial void LogPrimarySet(this ILogger logger, Guid imageId, Guid restaurantId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Ошибка при установке главного изображения")]
    internal static partial void LogSetPrimaryError(this ILogger logger, Exception ex);
}
