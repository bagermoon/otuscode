using Ardalis.Result;

using Mediator;

namespace RestoRate.RatingService.Application.UseCases.Rating.Recalculate;

public sealed record RecalculateRatingCommand(Guid RestaurantId) : ICommand<Result>;