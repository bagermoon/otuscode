using NodaMoney;

namespace RestoRate.RatingService.Domain.Models;

public sealed record RestaurantRatingSnapshot(
    Guid RestaurantId,
    decimal ApprovedAverageRating,
    int ApprovedReviewsCount,
    Money? ApprovedAverageCheck,
    decimal ProvisionalAverageRating,
    int ProvisionalReviewsCount,
    Money? ProvisionalAverageCheck
);
