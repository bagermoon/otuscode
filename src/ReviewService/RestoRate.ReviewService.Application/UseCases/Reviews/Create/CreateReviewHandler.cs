using Ardalis.Result;
using Ardalis.SharedKernel;

using Mediator;

using Microsoft.Extensions.Logging;

using RestoRate.Abstractions.Identity;
using RestoRate.Contracts.Review.Dtos;
using RestoRate.ReviewService.Application.Mappings;
using RestoRate.ReviewService.Domain.ReviewAggregate;

namespace RestoRate.ReviewService.Application.UseCases.Reviews.Create;

public sealed class CreateReviewHandler(
    IRepository<Review> repository,
    IUserContext userContext,
    ILogger<CreateReviewHandler> logger)
    : ICommandHandler<CreateReviewCommand, Result<ReviewDto>>
{
    public async ValueTask<Result<ReviewDto>> Handle(
        CreateReviewCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogHandling(request.Dto.RestaurantId, request.Dto.UserId);

        if (!userContext.IsAuthenticated || userContext.UserId == Guid.Empty)
        {
            return Result<ReviewDto>.Invalid(new[]
            {
                new ValidationError("UserId", "Authenticated user is required.")
            });
        }

        if (request.Dto.UserId == Guid.Empty)
        {
            return Result<ReviewDto>.Invalid(new[]
            {
                new ValidationError("UserId", "UserId must be set.")
            });
        }

        if (request.Dto.UserId != userContext.UserId)
        {
            return Result<ReviewDto>.Invalid(new[]
            {
                new ValidationError("UserId", "UserId must match the authenticated user.")
            });
        }

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

            logger.LogCreated(review.Id);
            return review.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogCreateError(ex);
            return Result<ReviewDto>.Error(ex.Message);
        }
    }
}
