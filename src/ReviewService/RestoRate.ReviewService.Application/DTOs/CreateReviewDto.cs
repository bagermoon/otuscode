using RestoRate.Contracts.Common.Dtos;

namespace RestoRate.ReviewService.Application.DTOs;

public record CreateReviewDto(
    Guid RestaurantId,
    Guid UserId,
    decimal Rating,
    MoneyDto? AverageCheck,
    string Text
);
