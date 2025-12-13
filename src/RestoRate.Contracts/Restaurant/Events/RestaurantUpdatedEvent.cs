using RestoRate.Abstractions.Messaging;

namespace RestoRate.Contracts.Restaurant.Events;

[Obsolete("RestaurantUpdatedEvent is deprecated.", false)]
public sealed record RestaurantUpdatedEvent(
    Guid RestaurantId,
    RestaurantStatus Status
) : IIntegrationEvent;
