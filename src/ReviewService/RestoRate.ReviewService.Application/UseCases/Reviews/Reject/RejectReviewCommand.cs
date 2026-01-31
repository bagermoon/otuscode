using Ardalis.Result;

using Mediator;

namespace RestoRate.ReviewService.Application.UseCases.Reviews.Reject;

public sealed record RejectReviewCommand(
    Guid ReviewId
) : ICommand<Result>;
