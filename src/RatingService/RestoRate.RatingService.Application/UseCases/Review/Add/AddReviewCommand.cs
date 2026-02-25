using Ardalis.Result;

using Mediator;

using NodaMoney;

namespace RestoRate.RatingService.Application.UseCases.Review.Add;

public sealed record AddReviewCommand(
    Guid ReviewId,
    Guid RestaurantId,
    decimal Rating,
    Money? AverageCheck
) : ICommand<Result>;
