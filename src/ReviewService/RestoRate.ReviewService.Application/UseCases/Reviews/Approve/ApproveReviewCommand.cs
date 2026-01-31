using Ardalis.Result;

using Mediator;

namespace RestoRate.ReviewService.Application.UseCases.Reviews.Approve;

public sealed record ApproveReviewCommand(
    Guid ReviewId
) : ICommand<Result>;
