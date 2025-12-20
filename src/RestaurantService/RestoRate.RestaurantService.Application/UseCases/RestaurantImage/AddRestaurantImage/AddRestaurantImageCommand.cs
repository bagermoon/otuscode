using Ardalis.Result;

using Mediator;

namespace RestoRate.RestaurantService.Application.UseCases.RestaurantImage.AddRestaurantImage;

public sealed record AddRestaurantImageCommand(
    Guid RestaurantId,
    string Url,
    string? AltText = null,
    bool IsPrimary = false
) : ICommand<Result<Guid>>;
