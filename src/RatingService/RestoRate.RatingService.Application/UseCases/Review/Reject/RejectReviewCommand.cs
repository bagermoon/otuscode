using Ardalis.Result;

using Mediator;

namespace RestoRate.RatingService.Application.UseCases.Review.Reject;

public sealed record RejectReviewCommand(
    Guid ReviewId
) : ICommand<Result>;
