namespace RestoRate.Shared.Contracts.Restaurant.Events;

public sealed record RestaurantUpdatedEvent(
    Guid RestaurantId,
    string Name,
    string? Description,
    string[] Tags);
