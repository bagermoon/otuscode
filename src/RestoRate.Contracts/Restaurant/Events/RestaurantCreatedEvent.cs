using RestoRate.Abstractions.Messaging;

namespace RestoRate.Contracts.Restaurant.Events;

public sealed record RestaurantCreatedEvent(
    Guid RestaurantId,
    string Name
) : IIntegrationEvent;
