using Ardalis.SharedKernel;

namespace RestoRate.Restaurant.Domain.RestaurantAggregate.Events;

public sealed class RestaurantDeletedEvent(int restaurantId) : DomainEventBase
{
    public int RestaurantId { get; init; } = restaurantId;
}
