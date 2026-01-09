using Ardalis.SharedKernel;

using Microsoft.Extensions.Logging;
using RestoRate.RestaurantService.Application.Mappings;
using RestoRate.Abstractions.Messaging;
using RestoRate.RestaurantService.Domain.RestaurantAggregate.Events;

namespace RestoRate.RestaurantService.Application.Handlers;

public sealed class RestaurantDeletedEventHandler(
    IIntegrationEventBus integrationEventBus,
    ILogger<RestaurantDeletedEventHandler> logger)
    : IDomainEventHandler<RestaurantDeletedEvent>
{
    public async ValueTask Handle(RestaurantDeletedEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogRestaurantDeleted(domainEvent.RestaurantId);

        await integrationEventBus.PublishAsync(
            new Contracts.Restaurant.Events.RestaurantArchivedEvent(
                RestaurantId: domainEvent.RestaurantId,
                Status: domainEvent.RestaurantStatus.ToContract()
            ), cancellationToken);
    }
}
