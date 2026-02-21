using NodaMoney;
using RestoRate.RatingService.Domain.ReviewReferenceAggregate;

namespace RestoRate.RatingService.Domain.Interfaces;
public interface IReviewReferenceRepository
{
    Task AddReviewReferenceAsync(ReviewReference reviewReference, CancellationToken cancellationToken = default);
    Task<ReviewReference?> GetReviewReferenceByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateReviewReferenceAsync(ReviewReference reviewReference, CancellationToken cancellationToken = default);
    Task DeleteReviewReferenceAsync(Guid id, CancellationToken cancellationToken = default);

    Task<decimal?> GetAverageRatingByRestaurantIdAsync(Guid restaurantId, bool approvedOnly = true, CancellationToken cancellationToken = default);
    Task<Money?> GetAverageCheckByRestaurantIdAsync(Guid restaurantId, bool approvedOnly = true, CancellationToken cancellationToken = default);
    Task<int> GetReviewsCountByRestaurantIdAsync(Guid restaurantId, bool approvedOnly = true, CancellationToken cancellationToken = default);
}