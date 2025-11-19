using Ardalis.Result;
using Ardalis.SharedKernel;

using Mediator;
using Microsoft.Extensions.Logging;
using RestoRate.Review.Application.DTOs;
using ReviewEntity = RestoRate.Review.Domain.ReviewAggregate.Review;

namespace RestoRate.Review.Application.UseCases.Create;

public sealed class CreateReviewHandler(
    IRepository<ReviewEntity> repository,
    ILogger<CreateReviewHandler> logger)
    : ICommandHandler<CreateReviewCommand, Result<ReviewDto>>
{
    public async ValueTask<Result<ReviewDto>> Handle(
        CreateReviewCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling CreateReviewCommand for RestaurantId: {RestaurantId}, UserId: {UserId}", request.Dto.RestaurantId, request.Dto.UserId);

        try
        {
            var reviewObjet = ReviewEntity.Create(
                request.Dto.RestaurantId,
                request.Dto.UserId,
                request.Dto.Rating,
                request.Dto.Text);


            var review = await repository.AddAsync(reviewObjet, cancellationToken);

            var dto = new ReviewDto(
                review.Id,
                review.RestaurantId,
                review.UserId,
                review.Rating,
                review.Text ?? string.Empty,
                review.CreatedAt,
                review.UpdatedAt);

            logger.LogInformation("Review created successfully: ID {ReviewId}", review.Id);
            return Result<ReviewDto>.Success(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating review");
            return Result<ReviewDto>.Error(ex.Message);
        }
    }
}
