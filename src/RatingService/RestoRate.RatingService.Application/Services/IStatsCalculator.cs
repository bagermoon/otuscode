using RestoRate.RatingService.Application.Models;

namespace RestoRate.RatingService.Application.Services;

public interface IStatsCalculator
{
    Task<bool> RecalculateDebouncedAsync(
        Guid restaurantId,
        bool requestedApprovedOnly,
        CancellationToken cancellationToken = default);

    Task<RatingRecalculationResult> RecalculateAsync(
        Guid restaurantId,
        bool requestedApprovedOnly,
        CancellationToken cancellationToken = default);
}
