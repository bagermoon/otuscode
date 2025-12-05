namespace RestoRate.ReviewService.Application.DTOs;

public record ReviewDto(
    Guid Id,
    Guid RestaurantId,
    Guid UserId,
    int Rating,
    string Text,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
