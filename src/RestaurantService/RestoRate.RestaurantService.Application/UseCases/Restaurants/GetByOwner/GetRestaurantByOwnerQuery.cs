using Ardalis.Result;

using Mediator;

using RestoRate.Contracts.Restaurant.DTOs;

namespace RestoRate.RestaurantService.Application.UseCases.Restaurants.GetByOwner;

public record GetRestaurantByOwnerQuery(Guid OwnerId) : IQuery<Result<List<RestaurantDto>>>;
