using Ardalis.SharedKernel;

using RestoRate.SharedKernel.Enums;

namespace RestoRate.RestaurantService.Domain.RestaurantAggregate.Events;

public sealed class RestaurantCreatedEvent(Restaurant restaurant) : DomainEventBase
{
    public Restaurant Restaurant { get; } = restaurant;
    public Guid RestaurantId => Restaurant.Id;
    public string Name => Restaurant.Name;
    public RestaurantStatus RestaurantStatus => Restaurant.RestaurantStatus;
}
