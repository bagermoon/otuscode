using NodaMoney;

using RestoRate.Contracts.Restaurant.DTOs;
using RestoRate.RestaurantService.Domain.RestaurantAggregate;

namespace RestoRate.RestaurantService.Application.Mappings;

public static class RatingDtoExtensions
{
    public static RatingDto ToDto(this RatingSnapshot rating)
    {
        if (rating == null) return new RatingDto(0m, 0, Money.Zero.ToDto(), 0m, 0, Money.Zero.ToDto());

        return new RatingDto(
            rating.AverageRate,
            rating.ReviewCount,
            rating.AverageCheck.ToDto(),
            rating.ProvisionalAverageRate,
            rating.ProvisionalReviewCount,
            rating.ProvisionalAverageCheck.ToDto()
        );
    }
}
