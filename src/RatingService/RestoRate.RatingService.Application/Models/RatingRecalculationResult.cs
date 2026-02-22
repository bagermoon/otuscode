using RestoRate.RatingService.Domain.Models;

namespace RestoRate.RatingService.Application.Models;

public sealed record RatingRecalculationResult(
    RestaurantRatingSnapshot Approved,
    RestaurantRatingSnapshot Provisional);
