using System;

using Ardalis.Specification;

using RestoRate.SharedKernel.Enums;

namespace RestoRate.ReviewService.Domain.ReviewAggregate.Specifications;

public static class ReviewSpecExtensions
{
    public static ISpecificationBuilder<Review> PendingReviews(
    this ISpecificationBuilder<Review> Query)
    {
        return Query.Where(r => r.Status == ReviewStatus.Pending);
    }
}
