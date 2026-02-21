using RestoRate.RatingService.Domain.Models;

namespace RestoRate.RatingService.Domain.Interfaces;

public interface IRestaurantRatingCache
{
    Task<RestaurantRatingSnapshot?> GetAsync(Guid restaurantId, CancellationToken cancellationToken = default);
    Task SetAsync(RestaurantRatingSnapshot snapshot, CancellationToken cancellationToken = default);
}
