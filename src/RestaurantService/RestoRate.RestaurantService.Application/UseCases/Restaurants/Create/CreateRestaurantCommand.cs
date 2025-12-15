using Ardalis.Result;

using Mediator;

using RestoRate.RestaurantService.Application.DTOs;
using RestoRate.RestaurantService.Application.DTOs.CRUD;

namespace RestoRate.RestaurantService.Application.UseCases.Restaurants.Create;

public record CreateRestaurantCommand(CreateRestaurantDto Dto) : ICommand<Result<RestaurantDto>>;
