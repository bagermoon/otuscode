using Ardalis.Result;

using Mediator;

namespace RestoRate.RatingService.Application.UseCases.Review.Approve;

public sealed record ApproveReviewCommand(
    Guid ReviewId
) : ICommand<Result>;
