using Ardalis.Specification;

using RestoRate.SharedKernel.Enums;

namespace RestoRate.ReviewService.Domain.ReviewAggregate.Specifications;

/// <summary>
/// Specification to select pending reviews for a given restaurant.
/// </summary>
public sealed class GetPendingReviewsSpec : Specification<Review>
{
    public GetPendingReviewsSpec(Guid? restaurantId = null)
    {
        Query.Where(r => r.Status == ReviewStatus.Pending);

        if (restaurantId.HasValue)
        {
            Query
                .Where(r => r.RestaurantId == restaurantId);
        }
    }
}