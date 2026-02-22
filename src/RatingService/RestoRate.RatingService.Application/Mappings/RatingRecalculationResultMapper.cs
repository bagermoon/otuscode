using RestoRate.Contracts.Rating.Events;
using RestoRate.RatingService.Application.Models;

namespace RestoRate.RatingService.Application.Mappings;

public static class RatingRecalculationResultMapper
{
    public static RestaurantRatingRecalculatedEvent ToEvent(this RatingRecalculationResult result)
    {
        var approved = result.Approved;
        var provisional = result.Provisional;

        return new RestaurantRatingRecalculatedEvent(
            RestaurantId: approved.RestaurantId,
            ApprovedAverageRating: approved.AverageRating,
            ApprovedReviewsCount: approved.ReviewsCount,
            ApprovedAverageCheck: approved.AverageCheck.ToDto(),
            ProvisionalAverageRating: provisional.AverageRating,
            ProvisionalReviewsCount: provisional.ReviewsCount,
            ProvisionalAverageCheck: provisional.AverageCheck.ToDto());
    }
}
