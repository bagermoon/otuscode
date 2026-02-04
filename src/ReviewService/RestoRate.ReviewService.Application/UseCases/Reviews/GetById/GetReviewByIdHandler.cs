using Ardalis.Result;

using Mediator;

using Microsoft.Extensions.Logging;

using RestoRate.Contracts.Review.Dtos;
using RestoRate.ReviewService.Application.Mappings;
using RestoRate.ReviewService.Domain.Interfaces;
using RestoRate.ReviewService.Domain.ReviewAggregate.Specifications;
namespace RestoRate.ReviewService.Application.UseCases.Reviews.GetById;

public sealed class GetReviewByIdHandler(
    IReviewRepository repository,
    ILogger<GetReviewByIdHandler> logger)
    : IQueryHandler<GetReviewByIdQuery, Result<ReviewDto>>
{
    public async ValueTask<Result<ReviewDto>> Handle(GetReviewByIdQuery request, CancellationToken cancellationToken)
    {
        logger.LogGetting(request.Id);
        var spec = new GetReviewByIdSpec(request.Id);
        var result = await repository.FirstOrDefaultAsync(spec, cancellationToken);

        if (result is null)
        {
            logger.LogNotFound(request.Id);
            return Result<ReviewDto>.NotFound();
        }

        return result.ToDto();
    }
}
