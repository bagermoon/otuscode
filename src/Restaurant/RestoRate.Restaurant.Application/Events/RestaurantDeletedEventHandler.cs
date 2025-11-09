using Ardalis.SharedKernel;

using Microsoft.Extensions.Logging;
using RestoRate.Restaurant.Domain.RestaurantAggregate.Events;

namespace RestoRate.Restaurant.Application.Events;

internal sealed class RestaurantDeletedEventHandler(ILogger<RestaurantDeletedEventHandler> logger)
    : IDomainEventHandler<RestaurantDeletedEvent>
{
    public ValueTask Handle(RestaurantDeletedEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation("Обработка события: Ресторан удален (ID: {RestaurantId})",
            domainEvent.RestaurantId);

        return ValueTask.CompletedTask;
    }
}
