using Ardalis.SharedKernel;

using Microsoft.Extensions.Logging;
using RestoRate.Restaurant.Domain.RestaurantAggregate.Events;

namespace RestoRate.Restaurant.Application.Handlers;

public sealed class RestaurantUpdatedEventHandler(ILogger<RestaurantUpdatedEventHandler> logger)
    : IDomainEventHandler<RestaurantUpdatedEvent>
{
    public ValueTask Handle(RestaurantUpdatedEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogRestaurantUpdated(domainEvent.Name, domainEvent.RestaurantId);

        return ValueTask.CompletedTask;
    }
}
