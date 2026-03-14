using Microsoft.Extensions.Logging;

namespace RestoRate.RestaurantService.Application.UseCases.RestaurantImage.RemoveRestaurantImage;

internal static partial class RemoveRestaurantImageHandlerLogger
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Удаление изображения {ImageId} из ресторана {RestaurantId}")]
    internal static partial void LogRemoving(this ILogger logger, Guid imageId, Guid restaurantId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Ресторан {RestaurantId} не найден")]
    internal static partial void LogRestaurantNotFound(this ILogger logger, Guid restaurantId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Изображение {ImageId} не найдено в ресторане {RestaurantId}")]
    internal static partial void LogImageNotFound(this ILogger logger, Guid imageId, Guid restaurantId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Изображение {ImageId} удалено из ресторана {RestaurantId}")]
    internal static partial void LogRemoved(this ILogger logger, Guid imageId, Guid restaurantId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Ошибка при удалении изображения")]
    internal static partial void LogRemoveError(this ILogger logger, Exception ex);
}
