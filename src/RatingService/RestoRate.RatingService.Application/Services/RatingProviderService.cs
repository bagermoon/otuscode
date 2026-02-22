using Microsoft.Extensions.Logging;

using RestoRate.RatingService.Application.Models;
using RestoRate.RatingService.Domain.Interfaces;

namespace RestoRate.RatingService.Application.Services;

public sealed class RatingProviderService(
    IRestaurantRatingCache ratingCache,
    IStatsCalculator statsCalculator)
    : IRatingProviderService
{
    public async Task<RatingRecalculationResult> GetRatingAsync(
        Guid restaurantId,
        CancellationToken cancellationToken = default)
    {
        var approvedSnapshot = await ratingCache.GetAsync(restaurantId, approvedOnly: true, cancellationToken);
        var provisionalSnapshot = await ratingCache.GetAsync(restaurantId, approvedOnly: false, cancellationToken);

        if (approvedSnapshot is null || provisionalSnapshot is null)
        {
            var recalculated = await statsCalculator.RecalculateAsync(
                restaurantId,
                requestedApprovedOnly: false,
                cancellationToken);

            approvedSnapshot = recalculated.Approved;
            provisionalSnapshot = recalculated.Provisional;
        }

        return new RatingRecalculationResult(
                Approved: approvedSnapshot,
                Provisional: provisionalSnapshot
        );
    }
}
