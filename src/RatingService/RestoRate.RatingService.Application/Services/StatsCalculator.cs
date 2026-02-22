using Microsoft.Extensions.Logging;

using RestoRate.RatingService.Application.Models;
using RestoRate.RatingService.Domain.Interfaces;
using RestoRate.RatingService.Domain.Models;
using RestoRate.RatingService.Domain.Services;

namespace RestoRate.RatingService.Application.Services;

public sealed class StatsCalculator(
    IRatingCalculatorService ratingCalculator,
    IRestaurantRatingCache ratingCache,
    IRatingRecalculationDebouncer debouncer,
    ILogger<StatsCalculator> logger)
    : IStatsCalculator
{
    private static readonly TimeSpan DebounceWindow = TimeSpan.FromSeconds(1);

    public async Task<bool> RecalculateDebouncedAsync(
        Guid restaurantId,
        bool requestedApprovedOnly,
        CancellationToken cancellationToken = default)
    {
        if (!await ShouldRecalculateAsync(restaurantId, cancellationToken))
        {
            return false;
        }

        _ = await RecalculateAsync(restaurantId, requestedApprovedOnly, cancellationToken);
        return true;
    }

    public async Task<RatingRecalculationResult> RecalculateAsync(
        Guid restaurantId,
        bool requestedApprovedOnly,
        CancellationToken cancellationToken = default)
    {
        var primarySnapshot = await ratingCalculator.CalculateAsync(restaurantId, requestedApprovedOnly, cancellationToken);
        await TryCacheAsync(primarySnapshot, requestedApprovedOnly, cancellationToken);

        var otherApprovedOnly = !requestedApprovedOnly;
        var otherSnapshot = await TryGetCachedAsync(restaurantId, otherApprovedOnly, cancellationToken);

        if (otherSnapshot is null)
        {
            otherSnapshot = await ratingCalculator.CalculateAsync(restaurantId, otherApprovedOnly, cancellationToken);
            await TryCacheAsync(otherSnapshot, otherApprovedOnly, cancellationToken);
        }

        var approvedSnapshot = requestedApprovedOnly ? primarySnapshot : otherSnapshot;
        var provisionalSnapshot = requestedApprovedOnly ? otherSnapshot : primarySnapshot;

        return new RatingRecalculationResult(approvedSnapshot, provisionalSnapshot);
    }

    private async Task<bool> ShouldRecalculateAsync(Guid restaurantId, CancellationToken cancellationToken)
    {
        try
        {
            return await debouncer.TryEnterWindowAsync(restaurantId, DebounceWindow, cancellationToken);
        }
        catch (Exception ex)
        {
            // Fail-open: if Redis is down, compute immediately (no debounce).
            logger.LogWarning(ex, "Failed to debounce rating recalculation for restaurant {RestaurantId}", restaurantId);
            return true;
        }
    }

    private async Task<RestaurantRatingSnapshot?> TryGetCachedAsync(
        Guid restaurantId,
        bool approvedOnly,
        CancellationToken cancellationToken)
    {
        try
        {
            return await ratingCache.GetAsync(restaurantId, approvedOnly, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to read rating cache for restaurant {RestaurantId}", restaurantId);
            return null;
        }
    }

    private async Task TryCacheAsync(
        RestaurantRatingSnapshot snapshot,
        bool approvedOnly,
        CancellationToken cancellationToken)
    {
        try
        {
            await ratingCache.SetAsync(snapshot, approvedOnly, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to update rating cache for restaurant {RestaurantId}", snapshot.RestaurantId);
        }
    }
}
