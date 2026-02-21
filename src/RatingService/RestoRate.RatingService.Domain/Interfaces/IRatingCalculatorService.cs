using RestoRate.RatingService.Domain.Models;

namespace RestoRate.RatingService.Domain.Services;

public interface IRatingCalculatorService
{
	Task<RestaurantRatingSnapshot> CalculateAsync(Guid restaurantId, CancellationToken cancellationToken = default);
}
