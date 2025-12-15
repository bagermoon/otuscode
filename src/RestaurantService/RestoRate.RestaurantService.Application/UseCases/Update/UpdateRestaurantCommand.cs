using Ardalis.Result;
using Mediator;

using RestoRate.Contracts.Restaurant.DTOs.CRUD;

namespace RestoRate.RestaurantService.Application.UseCases.Update;

public record UpdateRestaurantCommand(UpdateRestaurantDto Dto) : ICommand<Result>;
