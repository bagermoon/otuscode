using System;

using Ardalis.Specification;

namespace RestoRate.ReviewService.Domain.ReviewAggregate.Specifications;

public class GetReviewByIdSpec : Specification<Review>
{
    public GetReviewByIdSpec(Guid reviewId)
    {
        Query
            .Where(r => r.Id == reviewId);
    }
}
