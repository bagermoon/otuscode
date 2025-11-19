using MassTransit;

using RestoRate.Contracts.Restaurant.Events;

namespace RestoRate.Review.Api.Handlers;

public class RestaurantCreatedHandler(ILogger<RestaurantCreatedHandler> logger) : IConsumer<RestaurantCreatedEvent>
{
    public Task Consume(ConsumeContext<RestaurantCreatedEvent> context)
    {
        logger.LogInformation("Restaurant created: {RestaurantId}", context.Message.RestaurantId);
        return Task.CompletedTask;
    }
}
