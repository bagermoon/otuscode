using Microsoft.Extensions.Logging;

namespace RestoRate.RestaurantService.Application.UseCases.Restaurants.GetAll;

internal static partial class GetAllRestaurantsHandlerLogger
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Получение списка ресторанов: страница {Page}, размер {Size}, поиск: {Search}")]
    internal static partial void LogGettingList(this ILogger logger, int page, int size, string? search);

    [LoggerMessage(Level = LogLevel.Information, Message = "Найдено {Count} ресторанов из {Total}")]
    internal static partial void LogFoundCount(this ILogger logger, int count, int total);

    [LoggerMessage(Level = LogLevel.Error, Message = "Ошибка при получении списка ресторанов")]
    internal static partial void LogGetAllError(this ILogger logger, Exception ex);
}
