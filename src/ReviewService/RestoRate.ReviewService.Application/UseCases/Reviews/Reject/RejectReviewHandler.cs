using Ardalis.Result;
using Ardalis.SharedKernel;

using Mediator;

using RestoRate.ReviewService.Domain.ReviewAggregate;

namespace RestoRate.ReviewService.Application.UseCases.Reviews.Reject;

public sealed class RejectReviewHandler(
    IRepository<Review> repository)
    : ICommandHandler<RejectReviewCommand, Result>
{
    public async ValueTask<Result> Handle(
        RejectReviewCommand request,
        CancellationToken cancellationToken)
    {
        var review = await repository.GetByIdAsync(request.ReviewId, cancellationToken);
        if (review is null)
        {
            return Result.NotFound($"Отзыв {request.ReviewId} не найден.");
        }

        review.Reject();
        await repository.UpdateAsync(review, cancellationToken);

        return Result.Success();
    }
}
