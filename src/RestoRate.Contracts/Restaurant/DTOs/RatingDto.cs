
using RestoRate.Contracts.Common.Dtos;

namespace RestoRate.Contracts.Restaurant.DTOs;
public record RatingDto(
    decimal AverageRating,
    int ReviewsCount,
    MoneyDto AverageCheck,
    decimal ProvisionalAverageRating,
    int ProvisionalReviewsCount,
    MoneyDto ProvisionalAverageCheck
);
