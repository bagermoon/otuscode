using RestoRate.RatingService.Application.Models;

namespace RestoRate.RatingService.Application.Services;

public interface IStatsCalculator
{
    Task QueueRecalculationAsync(
        Guid restaurantId,
        CancellationToken cancellationToken = default);

    Task<RatingRecalculationResult> RecalculateAsync(
        Guid restaurantId,
        bool requestedApprovedOnly,
        CancellationToken cancellationToken = default);

    Task<RatingRecalculationResult> RecalculateLatestAsync(
        Guid restaurantId,
        CancellationToken cancellationToken = default);
}
