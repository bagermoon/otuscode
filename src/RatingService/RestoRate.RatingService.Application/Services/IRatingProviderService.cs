using RestoRate.RatingService.Domain.Models;

namespace RestoRate.RatingService.Application.Services;

public interface IRatingProviderService
{
    Task<RestaurantRatingSnapshot> GetRatingAsync(Guid restaurantId, CancellationToken cancellationToken = default);
    Task<RestaurantRatingSnapshot> RefreshRatingAsync(Guid restaurantId, CancellationToken cancellationToken = default);
}
