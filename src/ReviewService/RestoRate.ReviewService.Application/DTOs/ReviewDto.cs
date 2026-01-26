using RestoRate.Contracts.Common.Dtos;

namespace RestoRate.ReviewService.Application.DTOs;

public record ReviewDto(
    Guid Id,
    Guid RestaurantId,
    Guid UserId,
    decimal Rating,
    MoneyDto? AverageCheck,
    string Text,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
