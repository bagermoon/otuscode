using Ardalis.Result;

using Mediator;

using RestoRate.Contracts.Restaurant.DTOs;
using RestoRate.Contracts.Restaurant.DTOs.CRUD;

namespace RestoRate.RestaurantService.Application.UseCases.Restaurants.Create;

public record CreateRestaurantCommand(CreateRestaurantDto Dto) : ICommand<Result<RestaurantDto>>;
