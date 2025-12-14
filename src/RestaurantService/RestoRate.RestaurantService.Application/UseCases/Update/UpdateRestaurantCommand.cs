using Ardalis.Result;
using Mediator;
using RestoRate.RestaurantService.Application.DTOs;
using RestoRate.RestaurantService.Application.DTOs.CRUD;

namespace RestoRate.RestaurantService.Application.UseCases.Update;

public record UpdateRestaurantCommand(UpdateRestaurantDto Dto) : ICommand<Result>;
