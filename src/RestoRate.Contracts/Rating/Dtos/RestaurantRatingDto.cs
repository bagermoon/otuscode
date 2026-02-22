using RestoRate.Contracts.Common.Dtos;
namespace RestoRate.Contracts.Rating.Dtos;

public sealed record RestaurantRatingDto(
    Guid RestaurantId,
    decimal ApprovedAverageRating,
    int ApprovedReviewsCount,
    MoneyDto ApprovedAverageCheck,
    decimal ProvisionalAverageRating,
    int ProvisionalReviewsCount,
    MoneyDto ProvisionalAverageCheck
);