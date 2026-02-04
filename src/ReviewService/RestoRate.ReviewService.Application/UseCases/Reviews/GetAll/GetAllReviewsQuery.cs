using Ardalis.Result;

using Mediator;

using RestoRate.Contracts.Review.Dtos;
using RestoRate.SharedKernel.Enums;

namespace RestoRate.ReviewService.Application.UseCases.Reviews.GetAll;

public sealed record GetAllReviewsQuery(
    int PageNumber = 1,
    int PageSize = 20,
    ReviewStatus[]? Statuses = null
) : IQuery<Result<RestoRate.Contracts.Common.PagedResult<ReviewDto>>>;