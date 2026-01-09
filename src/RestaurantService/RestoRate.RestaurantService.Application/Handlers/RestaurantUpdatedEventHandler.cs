using Ardalis.SharedKernel;

using Microsoft.Extensions.Logging;
using RestoRate.RestaurantService.Application.Mappings;
using RestoRate.Abstractions.Messaging;
using RestoRate.RestaurantService.Domain.RestaurantAggregate.Events;

namespace RestoRate.RestaurantService.Application.Handlers;

public sealed class RestaurantUpdatedEventHandler(
    IIntegrationEventBus integrationEventBus,
    ILogger<RestaurantUpdatedEventHandler> logger)
    : IDomainEventHandler<RestaurantUpdatedEvent>
{
    public async ValueTask Handle(RestaurantUpdatedEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogRestaurantUpdated(domainEvent.Name, domainEvent.RestaurantId);

        await integrationEventBus.PublishAsync(
            new Contracts.Restaurant.Events.RestaurantUpdatedEvent(
                RestaurantId: domainEvent.RestaurantId,
                Status: domainEvent.RestaurantStatus.ToContract()
            ), cancellationToken);
    }
}
