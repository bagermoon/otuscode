namespace RestoRate.Contracts.Restaurant.Requests;

public sealed record GetRestaurantStatusResponse(
    Guid RestaurantId,
    bool Exists,
    RestaurantStatus Status);
