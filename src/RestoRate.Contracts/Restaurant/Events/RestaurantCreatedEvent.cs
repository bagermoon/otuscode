namespace RestoRate.Contracts.Restaurant.Events;

public sealed record RestaurantCreatedEvent(
    Guid RestaurantId,
    string Slug,
    string Name,
    string Cuisine);
