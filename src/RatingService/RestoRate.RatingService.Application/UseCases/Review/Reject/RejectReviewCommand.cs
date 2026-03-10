using Ardalis.Result;

using Mediator;

using NodaMoney;

namespace RestoRate.RatingService.Application.UseCases.Review.Reject;

public sealed record RejectReviewCommand(
    Guid ReviewId,
    Guid RestaurantId,
    decimal Rating,
    Money? AverageCheck
) : ICommand<Result>;
