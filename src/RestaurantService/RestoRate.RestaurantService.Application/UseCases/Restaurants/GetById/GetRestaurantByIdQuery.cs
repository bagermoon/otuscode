using Ardalis.Result;

using Mediator;

using RestoRate.RestaurantService.Application.DTOs;

namespace RestoRate.RestaurantService.Application.UseCases.Restaurants.GetById;

public record GetRestaurantByIdQuery(Guid RestaurantId) : IQuery<Result<RestaurantDto>>;
