using Ardalis.Result;
using Mediator;
using RestoRate.Restaurant.Application.DTOs;
using RestoRate.Restaurant.Application.DTOs.CRUD;

namespace RestoRate.Restaurant.Application.UseCases.Update;

public record UpdateRestaurantCommand(UpdateRestaurantDto Dto) : ICommand<Result>;
