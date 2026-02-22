using NodaMoney;

namespace RestoRate.RatingService.Domain.Models;

public sealed record RestaurantRatingSnapshot(
    Guid RestaurantId,
    decimal AverageRating,
    int ReviewsCount,
    Money AverageCheck
);
