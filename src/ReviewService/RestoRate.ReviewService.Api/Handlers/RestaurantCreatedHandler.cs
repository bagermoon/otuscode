using MassTransit;

using RestoRate.Abstractions.Identity;
using RestoRate.Contracts.Restaurant.Events;

namespace RestoRate.ReviewService.Api.Handlers;

public class RestaurantCreatedHandler(ILogger<RestaurantCreatedHandler> logger, IUserContext userContext) : IConsumer<RestaurantCreatedEvent>
{
    public Task Consume(ConsumeContext<RestaurantCreatedEvent> context)
    {
        logger.LogInformation("Restaurant created: {RestaurantId}, UserId: {UserId}", context.Message.RestaurantId, userContext.UserId);
        return Task.CompletedTask;
    }
}
