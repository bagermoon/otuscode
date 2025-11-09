using Ardalis.SharedKernel;

namespace RestoRate.Restaurant.Domain.RestaurantAggregate.Events;

public sealed class RestaurantCreatedEvent(int restaurantId, string name) : DomainEventBase
{
    public int RestaurantId { get; init; } = restaurantId;
    public string Name { get; init; } = name;
}
