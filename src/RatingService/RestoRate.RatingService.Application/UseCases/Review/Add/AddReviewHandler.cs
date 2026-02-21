using Ardalis.Result;

using Mediator;

using RestoRate.RatingService.Domain.Interfaces;

namespace RestoRate.RatingService.Application.UseCases.Review.Add;

public sealed class AddReviewHandler(
    IReviewReferenceService reviewService)
    : ICommandHandler<AddReviewCommand, Result>
{
    public async ValueTask<Result> Handle(AddReviewCommand request, CancellationToken cancellationToken)
    {
        await reviewService.AddAsync(
            request.ReviewId,
            request.RestaurantId,
            request.Rating,
            request.AverageCheck,
            cancellationToken);
        return Result.Success();
    }
}
