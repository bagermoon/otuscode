using Ardalis.Result;

using Mediator;

using NodaMoney;

namespace RestoRate.RatingService.Application.UseCases.Review.Approve;

public sealed record ApproveReviewCommand(
    Guid ReviewId,
    Guid RestaurantId,
    decimal Rating,
    Money? AverageCheck
) : ICommand<Result>;
