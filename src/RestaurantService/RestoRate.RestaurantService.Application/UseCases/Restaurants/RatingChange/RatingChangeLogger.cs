namespace RestoRate.RestaurantService.Application.UseCases.Restaurants.RatingChange;

using Microsoft.Extensions.Logging;

internal static partial class RatingChangeLogger
{
	[LoggerMessage(Level = LogLevel.Warning, Message = "Ресторан не найден для изменения рейтинга: ID {RestaurantId}")]
	internal static partial void LogRestaurantNotFound(this ILogger logger, Guid restaurantId);

	[LoggerMessage(Level = LogLevel.Error, Message = "Ошибка при обновлении рейтинга ресторана {RestaurantId}: {Message}")]
	internal static partial void LogRatingUpdateFailed(this ILogger logger, Exception ex, Guid restaurantId, string message);

}
