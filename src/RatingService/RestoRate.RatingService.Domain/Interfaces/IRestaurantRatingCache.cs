using RestoRate.RatingService.Domain.Models;

namespace RestoRate.RatingService.Domain.Interfaces;

public interface IRestaurantRatingCache
{
    Task<RestaurantRatingSnapshot?> GetAsync(
        Guid restaurantId,
        bool approvedOnly,
        CancellationToken cancellationToken = default);

    Task SetAsync(
        RestaurantRatingSnapshot snapshot,
        bool approvedOnly,
        CancellationToken cancellationToken = default);
}
