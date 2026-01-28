using Ardalis.Result;

using Mediator;

namespace RestoRate.RestaurantService.Application.UseCases.Restaurants.GetStatus;

public sealed record GetRestaurantStatusQuery(Guid RestaurantId)
    : IQuery<Result<GetRestaurantStatusResult>>;
