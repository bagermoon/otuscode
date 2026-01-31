using Microsoft.Extensions.Logging;

namespace RestoRate.ReviewService.Application.UseCases.RestaurantReferences.UpsertRestaurant;

internal static partial class UpsertRestaurantHandlerLogger
{
    [LoggerMessage(Level = LogLevel.Information, Message = "RestaurantReference уже существует для {RestaurantId}; возвращается текущее сохраненное значение.")]
    internal static partial void LogRestaurantReferenceAlreadyExists(this ILogger logger, Guid restaurantId);
}
