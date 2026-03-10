using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RestoRate.RatingService.Application.Configurations;
using RestoRate.RatingService.Application.Models;
using RestoRate.RatingService.Domain.Interfaces;
using RestoRate.RatingService.Domain.Models;
using RestoRate.RatingService.Domain.Services;

namespace RestoRate.RatingService.Application.Services;

public sealed class StatsCalculator(
    IRatingCalculatorService ratingCalculator,
    IRestaurantRatingCache ratingCache,
    IRatingRecalculationDebouncer debouncer,
    ILogger<StatsCalculator> logger,
    IOptionsMonitor<RatingServiceOptions> options)
    : IStatsCalculator
{
    private TimeSpan DebounceWindow => options.CurrentValue.DebounceWindow;

    public async Task QueueRecalculationAsync(
        Guid restaurantId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await debouncer.MarkChangedAsync(restaurantId, DebounceWindow, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.FailedToDebounce(ex, restaurantId);
        }
    }

    public Task<RatingRecalculationResult> RecalculateLatestAsync(
        Guid restaurantId,
        CancellationToken cancellationToken = default)
        => RecalculateFreshAsync(restaurantId, cancellationToken);

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

    private async Task<RatingRecalculationResult> RecalculateFreshAsync(
        Guid restaurantId,
        CancellationToken cancellationToken)
    {
        var approvedSnapshot = await ratingCalculator.CalculateAsync(restaurantId, approvedOnly: true, cancellationToken);
        await TryCacheAsync(approvedSnapshot, approvedOnly: true, cancellationToken);

        var provisionalSnapshot = await ratingCalculator.CalculateAsync(restaurantId, approvedOnly: false, cancellationToken);
        await TryCacheAsync(provisionalSnapshot, approvedOnly: false, cancellationToken);

        return new RatingRecalculationResult(approvedSnapshot, provisionalSnapshot);
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
            logger.FailedToReadCache(ex, restaurantId);
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
            logger.FailedToUpdateCache(ex, snapshot.RestaurantId);
        }
    }
}
