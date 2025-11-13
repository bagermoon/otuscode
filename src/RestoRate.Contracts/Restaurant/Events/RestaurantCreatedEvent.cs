using RestoRate.Abstractions.Messaging;

namespace RestoRate.Contracts.Restaurant.Events;

public sealed record RestaurantCreatedEvent(
    int RestaurantId,
    string Name,
    string? Slug = default,
    string? Cuisine = default
) : IIntegrationEvent;
