using Microsoft.Extensions.Logging;

namespace RestoRate.ReviewService.Application.UseCases.Reviews.GetAll;

internal static partial class GetAllReviewsHandlerLogger
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Получение списка отзывов: страница {Page}, размер {Size}, статусы: {Statuses}")]
    internal static partial void LogGettingList(this ILogger logger, int page, int size, string? statuses);

    [LoggerMessage(Level = LogLevel.Information, Message = "Найдено {Count} отзывов из {Total}")]
    internal static partial void LogFoundCount(this ILogger logger, int count, int total);

    [LoggerMessage(Level = LogLevel.Error, Message = "Ошибка при получении списка отзывов")]
    internal static partial void LogGetAllError(this ILogger logger, Exception ex);
}
