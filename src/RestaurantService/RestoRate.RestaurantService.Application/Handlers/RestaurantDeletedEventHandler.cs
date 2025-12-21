using Ardalis.SharedKernel;

using Microsoft.Extensions.Logging;

using RestoRate.RestaurantService.Domain.RestaurantAggregate.Events;

namespace RestoRate.RestaurantService.Application.Handlers;

public sealed class RestaurantDeletedEventHandler(ILogger<RestaurantDeletedEventHandler> logger)
    : IDomainEventHandler<RestaurantDeletedEvent>
{
    public ValueTask Handle(RestaurantDeletedEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogRestaurantDeleted(domainEvent.RestaurantId);

        return ValueTask.CompletedTask;
    }
}
