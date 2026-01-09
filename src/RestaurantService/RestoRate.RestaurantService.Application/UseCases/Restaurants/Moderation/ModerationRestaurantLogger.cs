using System;
using Microsoft.Extensions.Logging;

namespace RestoRate.RestaurantService.Application.UseCases.Restaurants.Moderation;

internal static partial class ModerationRestaurantLogger
{
	[LoggerMessage(Level = LogLevel.Warning, Message = "Restaurant with ID {RestaurantId} not found for moderation.")]
	internal static partial void LogRestaurantNotFound(this ILogger logger, Guid restaurantId);

	[LoggerMessage(Level = LogLevel.Warning, Message = "Failed to update status for restaurant with ID {RestaurantId}: {Error}")]
	internal static partial void LogFailedToUpdateStatus(this ILogger logger, Guid restaurantId, object? error);
}