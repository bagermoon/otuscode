namespace RestoRate.Review.Application.DTOs;

public record CreateReviewDto(
    Guid RestaurantId,
    Guid UserId,
    int Rating,
    string Text
);
