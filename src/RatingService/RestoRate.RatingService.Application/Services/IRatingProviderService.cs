using RestoRate.RatingService.Application.Models;

namespace RestoRate.RatingService.Application.Services;

public interface IRatingProviderService
{
    Task<RatingRecalculationResult> GetRatingAsync(Guid restaurantId, CancellationToken cancellationToken = default);
}
