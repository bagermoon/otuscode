using Ardalis.SharedKernel;

using Microsoft.Extensions.Logging;
using RestoRate.Restaurant.Domain.RestaurantAggregate.Events;

namespace RestoRate.Restaurant.Application.Events;

internal sealed class RestaurantCreatedEventHandler(ILogger<RestaurantCreatedEventHandler> logger)
    : IDomainEventHandler<RestaurantCreatedEvent>
{
    public ValueTask Handle(RestaurantCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation("Обработка события: Ресторан '{RestaurantName}' создан (ID: {RestaurantId})",
            domainEvent.Name,
            domainEvent.RestaurantId);

        return ValueTask.CompletedTask;
    }
}
