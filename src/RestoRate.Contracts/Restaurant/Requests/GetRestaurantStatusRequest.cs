namespace RestoRate.Contracts.Restaurant.Requests;

/// <summary>
/// Request the current status of a restaurant from RestaurantService.
/// </summary>
public sealed record GetRestaurantStatusRequest(
    Guid RestaurantId);
