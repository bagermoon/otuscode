using Ardalis.Result;
using Ardalis.SharedKernel;

using Mediator;

using RestoRate.ReviewService.Domain.ReviewAggregate;

namespace RestoRate.ReviewService.Application.UseCases.Reviews.Approve;

public sealed class ApproveReviewHandler(
    IRepository<Review> repository)
    : ICommandHandler<ApproveReviewCommand, Result>
{
    public async ValueTask<Result> Handle(
        ApproveReviewCommand request,
        CancellationToken cancellationToken)
    {
        var review = await repository.GetByIdAsync(request.ReviewId, cancellationToken);
        if (review is null)
        {
            return Result.NotFound($"Отзыв {request.ReviewId} не найден.");
        }

        review.Approve();
        await repository.UpdateAsync(review, cancellationToken);

        return Result.Success();
    }
}
