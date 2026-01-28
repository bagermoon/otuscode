using RestoRate.SharedKernel.Enums;

namespace RestoRate.RestaurantService.Application.UseCases.Restaurants.GetStatus;

public sealed record GetRestaurantStatusResult(
    Guid RestaurantId,
    bool Exists,
    RestaurantStatus Status);
