using Mediator;
using Microsoft.Extensions.Logging;
using RestoRate.Restaurant.Domain.RestaurantAggregate.Events;

namespace RestoRate.Restaurant.Domain.RestaurantAggregate.Handlers;

internal sealed class RestaurantCreatedEventHandler(ILogger<RestaurantCreatedEventHandler> logger)
    : INotificationHandler<RestaurantCreatedEvent>
{
    public async ValueTask Handle(RestaurantCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation("Обработка события: Ресторан '{RestaurantName}' создан (ID: {RestaurantId})",
            domainEvent.Name,
            domainEvent.RestaurantId);

        //

        await ValueTask.CompletedTask;
    }
}
