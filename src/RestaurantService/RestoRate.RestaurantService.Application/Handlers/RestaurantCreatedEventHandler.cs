using Ardalis.SharedKernel;

using Microsoft.Extensions.Logging;

using RestoRate.RestaurantService.Domain.RestaurantAggregate.Events;

namespace RestoRate.RestaurantService.Application.Handlers;

public sealed class RestaurantCreatedEventHandler(ILogger<RestaurantCreatedEventHandler> logger)
    : IDomainEventHandler<RestaurantCreatedEvent>
{
    public ValueTask Handle(RestaurantCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogRestaurantCreated(domainEvent.Name, domainEvent.RestaurantId);

        return ValueTask.CompletedTask;
    }
}
