using Ardalis.SharedKernel;

using Mediator;

namespace RestoRate.Restaurant.Domain.RestaurantAggregate.Events;

public sealed class RestaurantCreatedEvent(int restaurantId, string name) : DomainEventBase, INotification
{
    public int RestaurantId { get; init; } = restaurantId;
    public string Name { get; init; } = name;
}
