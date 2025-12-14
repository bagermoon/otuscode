using Ardalis.SharedKernel;

namespace RestoRate.RestaurantService.Domain.RestaurantAggregate.Events;

public sealed class RestaurantDeletedEvent(Restaurant restaurant) : DomainEventBase
{
    public Restaurant Restaurant { get; } = restaurant;
    public Guid RestaurantId => Restaurant.Id;
    public string Name => Restaurant.Name;
    public string RestaurantStatus => Restaurant.RestaurantStatus.Name;
}
