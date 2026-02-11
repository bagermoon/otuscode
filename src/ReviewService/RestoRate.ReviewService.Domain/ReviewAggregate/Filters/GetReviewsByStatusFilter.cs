using RestoRate.SharedKernel.Enums;
using RestoRate.SharedKernel.Filters;

namespace RestoRate.ReviewService.Domain.ReviewAggregate.Filters;

public record GetReviewsByStatusFilter(
    int? PageNumber,
    int? PageSize,
    ReviewStatus[]? Statuses = null) : BaseFilter(PageNumber, PageSize);
