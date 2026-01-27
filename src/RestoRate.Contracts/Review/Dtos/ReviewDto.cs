using RestoRate.Contracts.Common.Dtos;

namespace RestoRate.Contracts.Review.Dtos;

public record ReviewDto(
    Guid Id,
    Guid RestaurantId,
    Guid UserId,
    decimal Rating,
    MoneyDto? AverageCheck,
    string Comment,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
