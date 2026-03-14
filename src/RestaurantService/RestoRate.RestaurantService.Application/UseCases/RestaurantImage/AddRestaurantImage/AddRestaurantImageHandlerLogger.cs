using Microsoft.Extensions.Logging;

namespace RestoRate.RestaurantService.Application.UseCases.RestaurantImage.AddRestaurantImage;

internal static partial class AddRestaurantImageHandlerLogger
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Добавление изображения к ресторану {RestaurantId}: {Url}")]
    internal static partial void LogAdding(this ILogger logger, Guid restaurantId, string url);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Ресторан {RestaurantId} не найден")]
    internal static partial void LogRestaurantNotFound(this ILogger logger, Guid restaurantId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Изображение {ImageId} добавлено к ресторану {RestaurantId}")]
    internal static partial void LogAdded(this ILogger logger, Guid imageId, Guid restaurantId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Ошибка при добавлении изображения")]
    internal static partial void LogAddError(this ILogger logger, Exception ex);
}
