using Ardalis.Result;
using Mediator;

namespace RestoRate.Restaurant.Application.UseCases.Delete;

public record DeleteRestaurantCommand(int RestaurantId) : ICommand<Result>;
