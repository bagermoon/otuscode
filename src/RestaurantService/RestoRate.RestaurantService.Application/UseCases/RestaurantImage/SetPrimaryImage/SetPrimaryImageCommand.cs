using Ardalis.Result;
using Mediator;

namespace RestoRate.RestaurantService.Application.UseCases.RestaurantImage.SetPrimaryImage;

public sealed record SetPrimaryImageCommand(
    Guid RestaurantId,
    Guid ImageId
) : ICommand<Result>;
