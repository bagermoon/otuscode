using Microsoft.Extensions.Logging;

namespace RestoRate.Restaurant.Domain.Services;

internal static partial class RestaurantServiceLogger
{
	[LoggerMessage(
		Level = LogLevel.Information,
		Message = "Создание ресторана {RestaurantName}")]
	public static partial void LogCreateRestaurantStart(this ILogger logger, string restaurantName);

	[LoggerMessage(
		Level = LogLevel.Information,
		Message = "Ресторан {RestaurantName} создан с ID {RestaurantId}")]
	public static partial void LogCreateRestaurantCompleted(this ILogger logger, string restaurantName, Guid restaurantId);

	[LoggerMessage(
		Level = LogLevel.Information,
		Message = "Обновление ресторана {RestaurantId}")]
	public static partial void LogUpdateRestaurantStart(this ILogger logger, Guid restaurantId);

	[LoggerMessage(
		Level = LogLevel.Information,
		Message = "Ресторан {RestaurantId} обновлен")]
	public static partial void LogUpdateRestaurantCompleted(this ILogger logger, Guid restaurantId);

	[LoggerMessage(
		Level = LogLevel.Information,
		Message = "Удаление ресторана {RestaurantId}")]
	public static partial void LogDeleteRestaurantStart(this ILogger logger, Guid restaurantId);

	[LoggerMessage(
		Level = LogLevel.Information,
		Message = "Ресторан {RestaurantId} удален")]
	public static partial void LogDeleteRestaurantCompleted(this ILogger logger, Guid restaurantId);

	[LoggerMessage(
		Level = LogLevel.Warning,
		Message = "Ресторан {RestaurantId} не найден")]
	public static partial void LogRestaurantNotFound(this ILogger logger, Guid restaurantId);
}
