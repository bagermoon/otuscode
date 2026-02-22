using NodaMoney;

using RestoRate.RatingService.Domain.Interfaces;
using RestoRate.RatingService.Domain.Models;

namespace RestoRate.RatingService.Domain.Services;

public sealed class RatingCalculatorService(IReviewReferenceRepository repository) : IRatingCalculatorService
{
    public async Task<RestaurantRatingSnapshot> CalculateAsync(Guid restaurantId, bool approvedOnly, CancellationToken cancellationToken = default)
    {
        var approvedAvgRatingTask = repository.GetAverageRatingByRestaurantIdAsync(
            restaurantId,
            approvedOnly,
            cancellationToken);
        var approvedCountTask = repository.GetReviewsCountByRestaurantIdAsync(
            restaurantId,
            approvedOnly,
            cancellationToken);
        var approvedAvgCheckTask = repository.GetAverageCheckByRestaurantIdAsync(
            restaurantId,
            approvedOnly,
            cancellationToken);

        await Task.WhenAll(
            approvedAvgRatingTask,
            approvedCountTask,
            approvedAvgCheckTask);

        var approvedAverageRating = approvedAvgRatingTask.Result ?? 0m;
        var approvedReviewsCount = approvedCountTask.Result;
        var approvedAverageCheck = approvedAvgCheckTask.Result ?? Money.Zero;

        return new RestaurantRatingSnapshot(
            RestaurantId: restaurantId,
            AverageRating: approvedAverageRating,
            ReviewsCount: approvedReviewsCount,
            AverageCheck: approvedAverageCheck);
    }
}
