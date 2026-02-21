using Microsoft.Extensions.Logging;

using RestoRate.RatingService.Domain.Interfaces;
using RestoRate.RatingService.Domain.Models;
using RestoRate.RatingService.Domain.Services;

namespace RestoRate.RatingService.Application.Services;

public sealed class RatingProviderService(
    IRestaurantRatingCache ratingCache,
    IRatingCalculatorService ratingCalculator,
    ILogger<RatingProviderService> logger)
    : IRatingProviderService
{
    public async Task<RestaurantRatingSnapshot> GetRatingAsync(Guid restaurantId, CancellationToken cancellationToken = default)
    {
        try
        {
            var cached = await ratingCache.GetAsync(restaurantId, cancellationToken);
            if (cached is not null)
            {
                return cached;
            }
        }
        catch (Exception ex)
        {
            // Cache is a derived read model; do not fail reads because Redis is down.
            logger.LogWarning(ex, "Failed to read rating cache for restaurant {RestaurantId}", restaurantId);
        }

        return await RefreshRatingAsync(restaurantId, cancellationToken);
    }

    public async Task<RestaurantRatingSnapshot> RefreshRatingAsync(Guid restaurantId, CancellationToken cancellationToken = default)
    {
        var snapshot = await ratingCalculator.CalculateAsync(restaurantId, cancellationToken);

        try
        {
            await ratingCache.SetAsync(snapshot, cancellationToken);
        }
        catch (Exception ex)
        {
            // Cache is best-effort; return computed snapshot anyway.
            logger.LogWarning(ex, "Failed to update rating cache for restaurant {RestaurantId}", restaurantId);
        }

        return snapshot;
    }
}
