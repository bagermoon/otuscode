using Ardalis.Result;

using Mediator;

using RestoRate.RatingService.Domain.Interfaces;

namespace RestoRate.RatingService.Application.UseCases.Review.Reject;

public sealed class RejectReviewHandler(
    IReviewReferenceService reviewService)
    : ICommandHandler<RejectReviewCommand, Result>
{
    public async ValueTask<Result> Handle(RejectReviewCommand request, CancellationToken cancellationToken)
    {
        await reviewService.RejectAsync(request.ReviewId, cancellationToken);
        return Result.Success();
    }
}
