using RestoRate.Contracts.Review.Dtos;
using RestoRate.ReviewService.Domain.ReviewAggregate;

namespace RestoRate.ReviewService.Application.Mappings;

public static class ReviewDtoExtensions
{
    public static ReviewDto ToDto(this Review review)
    {
        return new ReviewDto(
            review.Id,
            review.RestaurantId,
            review.UserId,
            review.User?.ToDto(),
            review.Rating,
            review.AverageCheck?.ToDto(),
            review.Comment ?? string.Empty,
            review.CreatedAt,
            review.UpdatedAt
        );
    }
}
