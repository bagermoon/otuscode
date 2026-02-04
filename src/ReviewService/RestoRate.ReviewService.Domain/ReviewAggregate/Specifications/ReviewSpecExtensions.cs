using System;

using Ardalis.Specification;

using RestoRate.SharedKernel.Enums;
using RestoRate.SharedKernel.Filters;

namespace RestoRate.ReviewService.Domain.ReviewAggregate.Specifications;

public static class ReviewSpecExtensions
{
    public static ISpecificationBuilder<Review> PendingReviews(
        this ISpecificationBuilder<Review> Query)
    {
        return Query.Where(r => r.Status == ReviewStatus.Pending);
    }

    public static ISpecificationBuilder<Review> ApplyOrdering(this ISpecificationBuilder<Review> builder, BaseFilter? filter = null)
    {
        if (filter is null) return builder;

        // We want the "asc" to be the default, that's why the condition is reverted.
        var isAscending = !(filter.OrderBy?.Equals("desc", StringComparison.OrdinalIgnoreCase) ?? false);

        return filter.OrderBy switch
        {
            nameof(Review.CreatedAt) => isAscending ? builder.OrderBy(x => x.CreatedAt) : builder.OrderByDescending(x => x.CreatedAt),
            _ => builder
        };
    }
}
