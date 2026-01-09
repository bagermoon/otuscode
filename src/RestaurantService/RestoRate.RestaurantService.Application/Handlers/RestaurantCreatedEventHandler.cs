using Ardalis.SharedKernel;

using Microsoft.Extensions.Logging;
using RestoRate.RestaurantService.Application.Mappings;
using RestoRate.Abstractions.Messaging;
using RestoRate.RestaurantService.Domain.RestaurantAggregate.Events;
namespace RestoRate.RestaurantService.Application.Handlers;

public sealed class RestaurantCreatedEventHandler(
    IIntegrationEventBus integrationEventBus,
    ILogger<RestaurantCreatedEventHandler> logger)
    : IDomainEventHandler<RestaurantCreatedEvent>
{
    public async ValueTask Handle(RestaurantCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogRestaurantCreated(domainEvent.Name, domainEvent.RestaurantId);
        await integrationEventBus.PublishAsync(
            new Contracts.Restaurant.Events.RestaurantCreatedEvent(
                RestaurantId: domainEvent.RestaurantId,
                Status: domainEvent.RestaurantStatus.ToContract()
            ), cancellationToken);
}
}
