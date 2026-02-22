using RestoRate.RatingService.Domain.Models;

namespace RestoRate.RatingService.Domain.Services;

public interface IRatingCalculatorService
{
	Task<RestaurantRatingSnapshot> CalculateAsync(Guid restaurantId, bool approvedOnly, CancellationToken cancellationToken = default);
}
