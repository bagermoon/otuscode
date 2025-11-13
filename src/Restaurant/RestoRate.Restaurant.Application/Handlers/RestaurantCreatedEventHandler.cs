using Ardalis.SharedKernel;

using Microsoft.Extensions.Logging;
using RestoRate.Restaurant.Domain.RestaurantAggregate.Events;

namespace RestoRate.Restaurant.Application.Handlers;

public sealed class RestaurantCreatedEventHandler(ILogger<RestaurantCreatedEventHandler> logger)
    : IDomainEventHandler<RestaurantCreatedEvent>
{
    public ValueTask Handle(RestaurantCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogRestaurantCreated(domainEvent.Name, domainEvent.RestaurantId);

        return ValueTask.CompletedTask;
    }
}
