using Ardalis.Result;

using Mediator;

using RestoRate.ReviewService.Domain.ReviewAggregate;

namespace RestoRate.ReviewService.Application.UseCases.Reviews.Reject;

public sealed record RejectReviewCommand(
    Guid ReviewId,
    ReviewRejectionSource RejectionSource
) : ICommand<Result>;
