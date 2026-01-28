using Ardalis.Specification;

using RestoRate.SharedKernel.Enums;

namespace RestoRate.ReviewService.Domain.ReviewAggregate.Specifications;

/// <summary>
/// Specification to select pending reviews.
/// </summary>
public sealed class GetPendingReviewsByRestaurantSpec : Specification<Review>
{
    public GetPendingReviewsByRestaurantSpec(Guid restaurantId)
    {
        Query.PendingReviews().Where(r => r.RestaurantId == restaurantId);
    }
}