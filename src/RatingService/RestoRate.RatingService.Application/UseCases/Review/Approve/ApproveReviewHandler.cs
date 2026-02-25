using Ardalis.Result;

using Mediator;

using RestoRate.RatingService.Domain.Interfaces;

namespace RestoRate.RatingService.Application.UseCases.Review.Approve;

public sealed class ApproveReviewHandler(
    IReviewReferenceService reviewService)
    : ICommandHandler<ApproveReviewCommand, Result>
{
    public async ValueTask<Result> Handle(ApproveReviewCommand request, CancellationToken cancellationToken)
    {
        await reviewService.ApproveAsync(request.ReviewId, cancellationToken);
        return Result.Success();
    }
}
