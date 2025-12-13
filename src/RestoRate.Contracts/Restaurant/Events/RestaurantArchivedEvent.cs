using RestoRate.Abstractions.Messaging;

namespace RestoRate.Contracts.Restaurant.Events;

public sealed record RestaurantArchivedEvent(
    Guid RestaurantId,
    RestaurantStatus Status
) : IIntegrationEvent;
