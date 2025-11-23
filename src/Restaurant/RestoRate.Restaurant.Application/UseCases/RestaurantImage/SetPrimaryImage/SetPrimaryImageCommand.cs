using Ardalis.Result;
using Mediator;

namespace RestoRate.Restaurant.Application.UseCases.RestaurantImage.SetPrimaryImage;

public sealed record SetPrimaryImageCommand(
    Guid RestaurantId,
    Guid ImageId
) : ICommand<Result>;
