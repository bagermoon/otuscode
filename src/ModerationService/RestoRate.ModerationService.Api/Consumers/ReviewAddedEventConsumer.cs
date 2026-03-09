using MassTransit;

using RestoRate.Contracts.Review.Events;

using Mediator;

namespace RestoRate.ModerationService.Api.Consumers
{
    public class ReviewAddedEventConsumer(
        ISender sender,
        Logger<ReviewAddedEventConsumer> logger
    ) : IConsumer<ReviewAddedEvent>
    {
        public async Task Consume(ConsumeContext<ReviewAddedEvent> context)
        {
        }
    }
}
