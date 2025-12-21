using Ardalis.Result;
using Ardalis.SharedKernel;

using Mediator;

using Microsoft.Extensions.Logging;

using RestoRate.ReviewService.Application.DTOs;
using RestoRate.ReviewService.Domain.ReviewAggregate;

namespace RestoRate.ReviewService.Application.UseCases.Create;

public sealed class CreateReviewHandler(
    IRepository<Review> repository,
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
            var reviewObjet = Review.Create(
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
                review.Comment ?? string.Empty,
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
