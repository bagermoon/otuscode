using Ardalis.Result;

using Mediator;

namespace RestoRate.RestaurantService.Application.UseCases.RestaurantImage.RemoveRestaurantImage;

public sealed record RemoveRestaurantImageCommand(
    Guid RestaurantId,
    Guid ImageId
) : ICommand<Result>;
