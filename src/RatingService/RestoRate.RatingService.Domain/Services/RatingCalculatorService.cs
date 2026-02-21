using RestoRate.RatingService.Domain.Interfaces;
using RestoRate.RatingService.Domain.Models;

namespace RestoRate.RatingService.Domain.Services;

public sealed class RatingCalculatorService(IReviewReferenceRepository repository) : IRatingCalculatorService
{
    public async Task<RestaurantRatingSnapshot> CalculateAsync(Guid restaurantId, CancellationToken cancellationToken = default)
    {
        var approvedAvgRatingTask = repository.GetAverageRatingByRestaurantIdAsync(
            restaurantId,
            approvedOnly: true,
            cancellationToken);
        var approvedCountTask = repository.GetReviewsCountByRestaurantIdAsync(
            restaurantId,
            approvedOnly: true,
            cancellationToken);
        var approvedAvgCheckTask = repository.GetAverageCheckByRestaurantIdAsync(
            restaurantId,
            approvedOnly: true,
            cancellationToken);

        var provisionalAvgRatingTask = repository.GetAverageRatingByRestaurantIdAsync(
            restaurantId,
            approvedOnly: false,
            cancellationToken);
        var provisionalCountTask = repository.GetReviewsCountByRestaurantIdAsync(
            restaurantId,
            approvedOnly: false,
            cancellationToken);
        var provisionalAvgCheckTask = repository.GetAverageCheckByRestaurantIdAsync(
            restaurantId,
            approvedOnly: false,
            cancellationToken);

        await Task.WhenAll(
            approvedAvgRatingTask,
            approvedCountTask,
            approvedAvgCheckTask,
            provisionalAvgRatingTask,
            provisionalCountTask,
            provisionalAvgCheckTask);

        var approvedAverageRating = approvedAvgRatingTask.Result ?? 0m;
        var approvedReviewsCount = approvedCountTask.Result;
        var approvedAverageCheck = approvedAvgCheckTask.Result;

        var provisionalAverageRating = provisionalAvgRatingTask.Result ?? 0m;
        var provisionalReviewsCount = provisionalCountTask.Result;
        var provisionalAverageCheck = provisionalAvgCheckTask.Result;

        return new RestaurantRatingSnapshot(
            RestaurantId: restaurantId,
            ApprovedAverageRating: approvedAverageRating,
            ApprovedReviewsCount: approvedReviewsCount,
            ApprovedAverageCheck: approvedAverageCheck,
            ProvisionalAverageRating: provisionalAverageRating,
            ProvisionalReviewsCount: provisionalReviewsCount,
            ProvisionalAverageCheck: provisionalAverageCheck);
    }
}
