using Ardalis.Result;
using Ardalis.SharedKernel;

using Mediator;

using Microsoft.Extensions.Logging;

using RestoRate.Contracts.Review.Dtos;
using RestoRate.ReviewService.Application.Mappings;
using RestoRate.ReviewService.Domain.Interfaces;
using RestoRate.ReviewService.Domain.ReviewAggregate;
using RestoRate.ReviewService.Domain.ReviewAggregate.Specifications;
using RestoRate.SharedKernel.Filters;

namespace RestoRate.ReviewService.Application.UseCases.Reviews.GetAll;

public sealed class GetAllReviewsHandler(
    IReviewRepository readRepository,
    ILogger<GetAllReviewsHandler> logger)
    : IQueryHandler<GetAllReviewsQuery, Result<RestoRate.Contracts.Common.PagedResult<ReviewDto>>>
{
    public async ValueTask<Result<RestoRate.Contracts.Common.PagedResult<ReviewDto>>> Handle(
        GetAllReviewsQuery request,
        CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber;
        var pageSize = request.PageSize;

        var statusesText = request.Statuses is { Length: > 0 }
            ? string.Join(',', request.Statuses)
            : null;

        logger.LogGettingList(pageNumber, pageSize, statusesText);

        try
        {
            var filter = request.ToFilter();
            var spec = new GetReviewsByStatusReadOnlySpec(filter);

            var filtered = await readRepository.ListAsync(spec, filter, cancellationToken);

            var result = filtered.ToContractPagedResult(r => r.ToDto());

            logger.LogFoundCount(result.Items.Count, result.TotalCount);

            return Result<RestoRate.Contracts.Common.PagedResult<ReviewDto>>.Success(result);
        }
        catch (Exception ex)
        {
            logger.LogGetAllError(ex);
            return Result<RestoRate.Contracts.Common.PagedResult<ReviewDto>>.Error(ex.Message);
        }
    }
}
