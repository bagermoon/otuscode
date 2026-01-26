using Ardalis.Result;

using Mediator;

using RestoRate.Contracts.Restaurant;

namespace RestoRate.ReviewService.Application.UseCases.RestaurantReferences.UpsertRestaurant;

public record UpsertRestaurantCommand(
    Guid RestaurantId,
    RestaurantStatus Status
) : ICommand<Result<RestaurantStatus>>;
