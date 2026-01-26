using Ardalis.Result;

using Mediator;

namespace RestoRate.ReviewService.Application.UseCases.Reviews.MoveToModerationPending;

public sealed record MoveReviewToModerationPendingCommand(
    Guid ReviewId
) : ICommand<Result>;
