using Ardalis.Result;
using Ardalis.SharedKernel;

using Mediator;

using RestoRate.ReviewService.Domain.ReviewAggregate;

namespace RestoRate.ReviewService.Application.UseCases.Reviews.MoveToModerationPending;

public sealed class MoveReviewToModerationPendingHandler(
    IRepository<Review> repository)
    : ICommandHandler<MoveReviewToModerationPendingCommand, Result>
{
    public async ValueTask<Result> Handle(
        MoveReviewToModerationPendingCommand request,
        CancellationToken cancellationToken)
    {
        var review = await repository.GetByIdAsync(request.ReviewId, cancellationToken);
        if (review is null)
        {
            return Result.NotFound($"Отзыв {request.ReviewId} не найден.");
        }

        review.MoveToModerationPending();
        await repository.UpdateAsync(review, cancellationToken);

        return Result.Success();
    }
}
