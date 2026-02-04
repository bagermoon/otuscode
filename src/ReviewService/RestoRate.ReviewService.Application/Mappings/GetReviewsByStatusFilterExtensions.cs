using RestoRate.ReviewService.Application.UseCases.Reviews.GetAll;
using RestoRate.ReviewService.Domain.ReviewAggregate.Filters;

namespace RestoRate.ReviewService.Application.Mappings;

public static class GetReviewsByStatusFilterExtensions
{
    public static GetReviewsByStatusFilter ToFilter(this GetAllReviewsQuery query)
    {
        if (query is null) throw new ArgumentNullException(nameof(query));

        return new GetReviewsByStatusFilter(
            PageNumber: query.PageNumber,
            PageSize: query.PageSize,
            Statuses: query.Statuses);
    }
}
