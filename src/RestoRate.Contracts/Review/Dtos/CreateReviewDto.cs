using RestoRate.Contracts.Common.Dtos;

namespace RestoRate.Contracts.Review.Dtos;

public record CreateReviewDto(
    Guid RestaurantId,
    Guid UserId,
    decimal Rating,
    MoneyDto? AverageCheck,
    string Comment
);
