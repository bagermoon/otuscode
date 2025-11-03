using Ardalis.SharedKernel;

using MediatR;

namespace RestoRate.Restaurant.Domain.RestaurantAggregate.Events;

public sealed class RestaurantUpdatedEvent(int restaurantId, string name) : DomainEventBase, INotification
{
    public int RestaurantId { get; init; } = restaurantId;
    public string Name { get; init; } = name;
}
