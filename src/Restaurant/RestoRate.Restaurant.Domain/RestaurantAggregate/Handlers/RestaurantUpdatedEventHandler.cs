using MediatR;
using Microsoft.Extensions.Logging;
using RestoRate.Restaurant.Domain.RestaurantAggregate.Events;

namespace RestoRate.Restaurant.Domain.RestaurantAggregate.Handlers;

internal sealed class RestaurantUpdatedEventHandler(ILogger<RestaurantUpdatedEventHandler> logger)
    : INotificationHandler<RestaurantUpdatedEvent>
{
    public Task Handle(RestaurantUpdatedEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation("Обработка события: Ресторан '{RestaurantName}' обновлен (ID: {RestaurantId})",
            domainEvent.Name,
            domainEvent.RestaurantId);

        return Task.CompletedTask;
    }
}
