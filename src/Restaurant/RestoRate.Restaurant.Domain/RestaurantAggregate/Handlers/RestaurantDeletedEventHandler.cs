using MediatR;
using Microsoft.Extensions.Logging;
using RestoRate.Restaurant.Domain.RestaurantAggregate.Events;

namespace RestoRate.Restaurant.Domain.RestaurantAggregate.Handlers;

internal sealed class RestaurantDeletedEventHandler(ILogger<RestaurantDeletedEventHandler> logger)
    : INotificationHandler<RestaurantDeletedEvent>
{
    public Task Handle(RestaurantDeletedEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation("Обработка события: Ресторан удален (ID: {RestaurantId})",
            domainEvent.RestaurantId);

        return Task.CompletedTask;
    }
}
