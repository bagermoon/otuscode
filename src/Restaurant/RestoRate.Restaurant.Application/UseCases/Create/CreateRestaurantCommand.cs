using Ardalis.Result;
using Mediator;
using RestoRate.Restaurant.Application.DTOs;
using RestoRate.Restaurant.Application.DTOs.CRUD;

namespace RestoRate.Restaurant.Application.UseCases.Create;

public record CreateRestaurantCommand(CreateRestaurantDto Dto) : ICommand<Result<RestaurantDto>>;
