using Ardalis.SharedKernel;
using MediatR;

namespace RestoRate.Restaurant.Domain.RestaurantAggregate.Events;

public sealed class RestaurantDeletedEvent(int restaurantId) : DomainEventBase, INotification
{
    public int RestaurantId { get; init; } = restaurantId;
}
