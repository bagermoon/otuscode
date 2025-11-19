using MassTransit;

namespace RestoRate.Review.Api.Handlers;

public class RestaurantCreatedHandlerDefinition : ConsumerDefinition<RestaurantCreatedHandler>
{
    protected override void ConfigureConsumer(
    IReceiveEndpointConfigurator endpointConfigurator,
    IConsumerConfigurator<RestaurantCreatedHandler> consumerConfigurator,
    IRegistrationContext context
    )
{
    endpointConfigurator.UseMessageRetry(r => r.Intervals(500, 1000));
    endpointConfigurator.UseInMemoryOutbox(context);
}
}
