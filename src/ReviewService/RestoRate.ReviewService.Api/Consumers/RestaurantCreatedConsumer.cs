using MassTransit;

using Mediator;

using RestoRate.Contracts.Restaurant.Events;
using RestoRate.ReviewService.Application.UseCases.RestaurantReferences.RestaurantReferenceValidation;

namespace RestoRate.ReviewService.Api.Consumers;

public sealed class RestaurantCreatedConsumer(
    ISender sender
)
    : IConsumer<RestaurantCreatedEvent>
{
    public async Task Consume(ConsumeContext<RestaurantCreatedEvent> context)
    {
        var message = context.Message;

        await sender.Send(
            new RestaurantReferenceValidationCommand(
                RestaurantId: message.RestaurantId,
                KnownStatus: message.Status,
                Exists: true),
            context.CancellationToken);
    }
}