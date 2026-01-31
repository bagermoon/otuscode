using Ardalis.Specification;

using RestoRate.SharedKernel.Enums;

namespace RestoRate.ReviewService.Domain.ReviewAggregate.Specifications;

/// <summary>
/// Specification to select pending reviews for a given user.
/// </summary>
public sealed class GetPendingReviewsByUserSpec : Specification<Review>
{
    public GetPendingReviewsByUserSpec(Guid userId)
    {
        Query.PendingReviews().Where(r => r.UserId == userId);
    }
}
