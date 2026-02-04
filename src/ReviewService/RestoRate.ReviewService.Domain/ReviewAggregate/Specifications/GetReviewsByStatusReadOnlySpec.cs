using Ardalis.Specification;

using RestoRate.ReviewService.Domain.ReviewAggregate.Filters;

namespace RestoRate.ReviewService.Domain.ReviewAggregate.Specifications;

public sealed class GetReviewsByStatusReadOnlySpec : Specification<Review>
{
    public GetReviewsByStatusReadOnlySpec(
        GetReviewsByStatusFilter filter)
    {
        Query.AsNoTracking();
        Query.ApplyOrdering(filter);

        if (filter.Statuses is { Length: > 0 })
        {
            var statusesSet = filter.Statuses.ToHashSet();
            Query.Where(r => statusesSet.Contains(r.Status));
        }
    }
}
