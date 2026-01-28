using Ardalis.Result;
using Ardalis.SharedKernel;

using Mediator;

using Microsoft.Extensions.Logging;

using RestoRate.Contracts.Review.Dtos;
using RestoRate.ReviewService.Application.Mappings;
using RestoRate.ReviewService.Domain.ReviewAggregate;

namespace RestoRate.ReviewService.Application.UseCases.Reviews.Create;

public sealed class CreateReviewHandler(
    IRepository<Review> repository,
    ILogger<CreateReviewHandler> logger)
    : ICommandHandler<CreateReviewCommand, Result<ReviewDto>>
{
    public async ValueTask<Result<ReviewDto>> Handle(
        CreateReviewCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogHandling(request.Dto.RestaurantId, request.Dto.UserId);

        try
        {
            var averageCheck = request.Dto.AverageCheck?.ToDomainMoney();

            var reviewObjet = Review.Create(
                request.Dto.RestaurantId,
                request.Dto.UserId,
                request.Dto.Rating,
                averageCheck,
                request.Dto.Comment);


            var review = await repository.AddAsync(reviewObjet, cancellationToken);

            var dto = new ReviewDto(
                review.Id,
                review.RestaurantId,
                review.UserId,
                review.Rating,
                review.AverageCheck?.ToDto(),
                review.Comment ?? string.Empty,
                review.CreatedAt,
                review.UpdatedAt);

            logger.LogCreated(review.Id);
            return Result<ReviewDto>.Success(dto);
        }
        catch (Exception ex)
        {
            logger.LogCreateError(ex);
            return Result<ReviewDto>.Error(ex.Message);
        }
    }
}
