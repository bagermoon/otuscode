using Ardalis.Result;

using Mediator;

using RestoRate.Contracts.Restaurant.DTOs;

namespace RestoRate.RestaurantService.Application.UseCases.Restaurants.GetById;

public record GetRestaurantByIdQuery(Guid RestaurantId) : IQuery<Result<RestaurantDto>>;
