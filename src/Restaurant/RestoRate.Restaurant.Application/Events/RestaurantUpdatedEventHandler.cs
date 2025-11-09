using Ardalis.SharedKernel;

using Microsoft.Extensions.Logging;
using RestoRate.Restaurant.Domain.RestaurantAggregate.Events;

namespace RestoRate.Restaurant.Application.Events;

internal sealed class RestaurantUpdatedEventHandler(ILogger<RestaurantUpdatedEventHandler> logger)
    : IDomainEventHandler<RestaurantUpdatedEvent>
{
    public ValueTask Handle(RestaurantUpdatedEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation("Обработка события: Ресторан '{RestaurantName}' обновлен (ID: {RestaurantId})",
            domainEvent.Name,
            domainEvent.RestaurantId);

        return ValueTask.CompletedTask;
    }
}
