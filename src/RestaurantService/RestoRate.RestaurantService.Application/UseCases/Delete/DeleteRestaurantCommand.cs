using Ardalis.Result;
using Mediator;

namespace RestoRate.RestaurantService.Application.UseCases.Delete;

public record DeleteRestaurantCommand(Guid RestaurantId) : ICommand<Result>;
